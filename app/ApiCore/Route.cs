using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Route
    {
        private static readonly Random Random = new Random();
        static decimal singleKMX = (decimal) 0.01453433399999998;
        static decimal singleKMY = (decimal) 0.00898315300000263;

        private static (decimal, decimal) NudgeGeo(double distanceKm, double angle, decimal latitude, decimal longitude)
        {
            const double radiusKm = 6378.137;

            const double pi = Math.PI;

            double distanceY = Math.Round(distanceKm * Math.Cos(angle / 360 * 2 * pi));
            double distanceX = Math.Round(distanceKm * Math.Sin(angle / 360 * 2 * pi));


            // North+ South- latitude (y axis)
            double newLatitude = Math.Round((double) latitude + distanceY / radiusKm * (180 / pi), 9);

            // East+ West- longitude (x axis)
            double newLongitude = Math.Round((double) longitude +
                                             distanceX / radiusKm * (180 / pi) / Math.Cos(newLatitude * pi / 180), 9);
            

            return ((decimal) newLatitude, (decimal) newLongitude);
        }

        public static string Distance(IDictionary<string, string> query)
        {
            // receive a list of postcodes in the query string and calculate the distances
            // between the route stops.

            foreach (var pair in query)
            {
                Console.WriteLine($"<{pair.Key}>, <{pair.Value}>");
            }
            
            List<StopData> postCodeList = new List<StopData>();

            if (query.Count > 0)
            {
                int seqNo = 0;
                foreach (var (_, postCode) in query)
                {
                    StopData stopData = new StopData
                    {
                        SeqNo = seqNo,
                        PostCode = postCode,
                        Valid = false,
                        Latitude = 0,
                        Longitude = 0,
                        Distance = 0
                    };

                    postCodeList.Add(stopData);
                    seqNo += 1;
                }
            }

            foreach (StopData r in postCodeList)
            {
                (bool isValid, decimal latitude, decimal longitude) = FetchGeoLocation(r.PostCode);

                if (!isValid) continue;

                r.Valid = true;
                r.Latitude = latitude;
                r.Longitude = longitude;
            }

            // now calculate distances

            StopData lastData = null;
            foreach (StopData r in postCodeList)
            {
                if (lastData != null)
                {
                    r.Distance = HaversineDistance(lastData.Latitude, lastData.Longitude, r.Latitude, r.Longitude);
                }

                Console.WriteLine($"{r.SeqNo}, {r.Valid}, {r.PostCode}, {r.Latitude}, {r.Longitude}, {r.Distance}");


                lastData = r;
            }


            return Result(200, "route simulated.", null);
        }

        private static (bool, decimal, decimal) FetchGeoLocation(string lookupCode)
        {
            Dictionary<string, string> codeQuery = new Dictionary<string, string>
            {
                {"postcode", lookupCode}
            };

            JObject postCode = JObject.Parse(TableName.Get("postcodes", codeQuery));

            foreach (JToken code in postCode["result"])
            {
                return (true, decimal.Parse(code["latitude"]?.ToString() ?? string.Empty),
                    decimal.Parse(code["longitude"].ToString()));
            }

            return (false, 0, 0);
        }

        public static string Simulate(IDictionary<string, string> query)
        {
            
            foreach (string date in CreateFutureDates())
            {
                // For n dates in advance
                JObject sqlDepots = JObject.Parse(TableName.Get("depots", null));

                foreach (JToken depotRow in sqlDepots["result"])
                {
                    // For all depots


                    JToken depotId = depotRow["depotId"];
                    int fleetSize = int.Parse(depotRow["fleetSize"].ToString());

                    string sqlText = $"select * from drivers where depotId = {depotId} order by rand()";
                    JObject driverRow = JObject.Parse(Database.GetSqlSelect(sqlText));

                    int noRoutesToday = fleetSize - RandomNumber(0, 4);

                    for (int i = 1; i <= noRoutesToday; i += 1)
                    {
                        // for a select number of drivers from this depot up
                        // to the total fleetSize of the depot.

                        if (!SingleRoute(date, depotRow, driverRow))
                        {
                            return Result(240, $"single route failed on {depotId}", null);
                        }
                    }

                    break;
                }

                break;
            }

            return Result(232, "route simulated.", null);
        }

        private static bool SingleRoute(string date, JToken depotRow, JToken driverRow)
        {
            
            List<StopData> stopsList = new List<StopData>();

            string depotPostCode = depotRow["postcode"].ToString();
            string sqlText = $"select latitude, longitude from postcodes where postcode = '{depotPostCode}' limit 1";
            
            JObject geolocation = JObject.Parse(Database.GetSqlSelect(sqlText));
            JToken result = geolocation["result"];

            decimal depotLat;
            decimal depotLong;
            
            if (result.HasValues)
            {
                depotLat = decimal.Parse(result[0]["latitude"].ToString());
                depotLong = decimal.Parse(result[0]["longitude"].ToString());
            }
            else
            {
                Console.WriteLine($"depot postcode not valid {depotPostCode}");
                return false;
            }
            
            int noStops = RandomNumber(15, 27);
            noStops = 5;

            string[] itemArray = {"fridge", "freezer", "washer", "dryer", "oven", "cooker", "dishwasher"};
            
            for (int routeSeqNo = 0; routeSeqNo <= noStops; routeSeqNo += 1)
            {
                JObject rowDetails = new JObject
                {
                    {"depotId", depotRow["depotId"]},
                    {"driverId", driverRow["driverId"]},
                    {"routeDate", date},
                    {"routeSeqNo", routeSeqNo},

                    {"addressId", 0},
                    {"addressPostcode", depotPostCode},

                    {"routeAction", routeSeqNo == 0 ? "depot" : "delivery"},
                    {"itemType", routeSeqNo == 0 ? "depot" : itemArray[RandomNumber(0, itemArray.Length - 1)]},
                    {"status", "pending"},
                    {
                        "refNo",
                        $"D{depotRow["depotId"]:0#}R{driverRow["driverId"]:0#}S{routeSeqNo:0#}T{date.Substring(date.Length - 2)}"
                    }
                };

                
                StopData stopData = new StopData
                {
                    SeqNo = routeSeqNo,
                    PostCode = depotPostCode,
                    Valid = false,
                    Latitude = 0,
                    Longitude = 0,
                    Distance = 0,
                    RowDetails = rowDetails
                };

                stopsList.Add(stopData);
            }

            // allocate new postcodes for route stops list by moving the geolocation forward
            // by a set amount of km. 
            
            int directionAngle = RandomNumber(0, 360);
            
            decimal latitude = depotLat;
            decimal longitude = depotLong;
            
            foreach (StopData stop in stopsList)
            {
                stop.Latitude = latitude;
                stop.Longitude = longitude;
                
                double kmDistance = 1.5 + (double) RandomNumber(-5, 5) / 10;
                double deviation = RandomNumber(-5, 5);

                (decimal newLat, decimal newLong) =
                    NudgeGeo(kmDistance, directionAngle+deviation, stop.Latitude, stop.Longitude);
                
                Console.WriteLine($"{stop.SeqNo}: {newLat}, {newLong}");

                latitude = newLat;
                longitude = newLong;
                
                // now find a nearby postcode
                
                sqlText = $"select postcode from postcodes where (latitude between {latitude-singleKMY} and {latitude+singleKMY}) and (longitude between {longitude-singleKMX} and {longitude+singleKMX}) limit 1";
                JObject newGeoArea = JObject.Parse(Database.GetSqlSelect(sqlText));
                result = newGeoArea["result"];

                if (result.HasValues)
                {
                    stop.PostCode = result[0]["postcode"].ToString();
                    stop.RowDetails["postcode"] = stop.PostCode;
                }

                Console.WriteLine(stop.RowDetails);
                
            }

            return true;
        }

        private static List<string> CreateFutureDates()
        {
            List<string> forwardDays = new List<string>();

            DateTime currentDay = DateTime.Now.Date;
            for (int i = 1; i <= 7; i += 1)
            {
                forwardDays.Add($"{currentDay.AddDays(i):yyyy-MM-dd}");
            }

            return forwardDays;
        }

        private static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max + 1);
        }


        private static decimal HaversineDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double radiusKm = 6378.137;

            double latitudeDelta = ToRadians((double) (lat2 - lat1));
            double longitudeDelta = ToRadians((double) (lon2 - lon1));

            double a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                       Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2) *
                       Math.Cos(ToRadians((double) lat1)) * Math.Cos(ToRadians((double) lat2));
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return (decimal) Math.Round(radiusKm * c, 4); // km
        }


        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }


        private class StopData
        {
            public int SeqNo { get; set; }
            public string PostCode { get; set; }
            public bool Valid { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public decimal Distance { get; set; }
            public JObject RowDetails { get; set; }
        }
    }
}