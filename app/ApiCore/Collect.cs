using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;
using static ApiCore.Route;

namespace ApiCore
{
    public static class Collect
    {
        private static readonly Random Random = new Random();

        public static string Request(IDictionary<string, string> query)
        {
            JArray collectDatesList = new JArray();
            string postcode = "";

            // look for collection postcode in querystring
            foreach (var pair in query)
            {
                // postcode should be the only key for this request
                if (pair.Key == "postcode")
                {
                    postcode = pair.Value;
                    break;
                }
            }

            if (postcode.Length == 0)
            {
                return Result(200, "no collection dates, missing postcode", null);
            }

            // customer postcode received, so look up
            // nearest depot to customer address
            
            JToken geoLocation = Database.SqlSingleRow($"select latitude, longitude from postcodes where postcode = '{postcode}' limit 1");

            decimal custLat = 0;
            decimal custLong = 0;

            if (geoLocation.HasValues)
            {
                custLat = decimal.Parse(geoLocation["latitude"].ToString());
                custLong = decimal.Parse(geoLocation["longitude"]?.ToString());
            }
            else
            {
                return Result(200, "no collection dates, invalid customer postcode", null);
            }

            JToken depotGeoSelect =
                Database.SqlMultiRow(
                    "select d.depotId, p.latitude, p.longitude from depots d, postcodes p where d.postcode = p.postcode");
            
            int closestDepotId = 0;
            decimal closestDistance = -1;

            foreach (var depotRow in depotGeoSelect)
            {
                var depotId = int.Parse(depotRow["depotId"].ToString());
                var depotLat = decimal.Parse(depotRow["latitude"].ToString());
                var depotLong = decimal.Parse(depotRow["longitude"]?.ToString());

                decimal distanceToDepot = HaversineDistance(custLat, custLong, depotLat, depotLong);

                if (closestDistance < 0)
                {
                    closestDistance = distanceToDepot;
                    closestDepotId = depotId;
                }
                else if (distanceToDepot < closestDistance)
                {
                    closestDistance = distanceToDepot;
                    closestDepotId = depotId;
                }
            }

            // find all dates that this depot has routes for
            JToken depotDates =
                Database.SqlMultiRow(
                    $"select distinct(date_format(routeDate,'%Y-%m-%d')) as routeDate from routes where depotId = {closestDepotId}");

            if (depotDates.Any())
            {
                // find the last stop postcode for each date/driver for this depot  
                foreach (var dateRow in depotDates)
                {
                    JToken maxStops =
                        Database.SqlMultiRow(
                            $"select max(routeSeqNo) as routeSeqNo, date_format(routeDate,'%Y-%m-%d') as routeDate, driverId from routes where depotId = {closestDepotId} and date_format(routeDate,'%Y-%m-%d') = '{dateRow["routeDate"]}' group by routeDate, driverId order by routeDate, driverId");

                    if (maxStops.HasValues)
                    {
                        
                        foreach (var maxStopRow in maxStops)
                        {
                            string routeDate = maxStopRow["routeDate"].ToString();
                            int lastStopNo = int.Parse(maxStopRow["routeSeqNo"].ToString());
                            int driverId = int.Parse(maxStopRow["driverId"].ToString());

                            JToken stopRow = Database.SqlSingleRow($"select latitude, longitude from routes where routeSeqNo = {lastStopNo} and date_format(routeDate,'%Y-%m-%d') = '{routeDate}' and driverId = {driverId} limit 1");

                            decimal stopLat = 0;
                            decimal stopLong = 0;

                            if (stopRow.HasValues)
                            {
                                stopLat = decimal.Parse(stopRow["latitude"].ToString());
                                stopLong = decimal.Parse(stopRow["longitude"]?.ToString());
                            }

                            decimal stopDistanceToCustomer = HaversineDistance(custLat, custLong, stopLat, stopLong);

                            if (stopDistanceToCustomer <= 10)
                            {
                                // these current final stops are within 10 km of the
                                // customer on this date, so supply the date to the customer
                                
                                collectDatesList.Add(routeDate);
                            }
                        }
                    }
                }
            }

            JArray finalDates= new JArray(collectDatesList.Distinct());
            
            return finalDates.Count == 0
                ? Result(200, "no collection dates available", finalDates)
                : Result(200, $"{finalDates.Count} collection dates available", finalDates);
        }

        public static string Confirm(string body)
        {
            JObject newAddress;
            string itemType = "";

            try
            {
                newAddress = JObject.Parse(body);

                if (newAddress["itemType"] != null)
                {
                    itemType = newAddress["itemType"].ToString();
                    newAddress.Remove("itemType");
                }

                if (newAddress["postcode"] != null)
                {
                    newAddress["postcode"] = newAddress["postcode"].ToString().ToUpper();
                }
            }
            catch
            {
                return Result(217, "collect-confirm internal error", null);
            }

            JObject postInfo =
                JObject.Parse(TableName.Post("addresses", newAddress.ToObject<Dictionary<string, string>>()));

            if (postInfo["status"].ToString() != "200")
            {
                return postInfo.ToString();
            }

            try
            {
                int addressId = int.Parse(postInfo["result"][0]["insertId"].ToString());

                newAddress["addressId"] = addressId;
                return Result(200, $"collection confirmed with address {{'addressId': {addressId}}}",
                    new JArray {newAddress});
            }
            catch
            {
                newAddress["addressId"] = -1;
            }


            return Result(230, "collection not confirmed, address database insert failure", new JArray {newAddress});
        }

        public static string Update()
        {
            return Result(201, "collection update", null);
        }

        public static string Cancel()
        {
            return Result(201, "collection cancelled", null);
        }


        private static JArray GetCollectionDates()
        {
            DateTime currentDay = DateTime.Now.Date;

            List<DateTime> forwardDays = new List<DateTime>();
            JArray collectDatesList = new JArray();

            for (int i = 1; i <= 7; i += 1)
            {
                forwardDays.Add(currentDay.AddDays(i));
            }

            int randDays = RandomNumber(2, 7);

            for (int i = 0; i < randDays; i += 1)
            {
                if (RandomNumber(3, 4) > 3) continue;

                string date = $"{forwardDays[i]:yyyy-MM-dd}";
                int depotId = RandomNumber(6, 9);
                int driverId = RandomNumber(1, 14);
                int routeSeqNo = RandomNumber(16, 30);
                string refNo = $"D{depotId:0#}R{driverId:0#}S{routeSeqNo:0#}T{date.Substring(date.Length - 2)}";

                JObject reply = new JObject
                {
                    {"date", date},
                    {"depotId", depotId},
                    {"driverId", driverId},
                    {"routeSeqNo", routeSeqNo},
                    {"refNo", refNo}
                };

                collectDatesList.Add(reply);
            }

            return collectDatesList;
        }

        private static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max + 1);
        }
    }
}