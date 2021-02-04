using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Route
    {
        private static readonly Random Random = new Random();


        public static string Distance(IDictionary<string, string> query)
        {
            foreach (var pair in query)
            {
                Console.WriteLine($"<{pair.Key}>, <{pair.Value}>");
            }


            List<RouteData> postCodeList = new List<RouteData>();

            if (query.Count > 0)
            {
                int seqNo = 0;
                foreach (var (_, postCode) in query)
                {
                    RouteData routeData = new RouteData
                    {
                        SeqNo = seqNo,
                        PostCode = postCode,
                        Valid = false,
                        Latitude = 0,
                        Longitude = 0,
                        Distance = 0
                    };

                    postCodeList.Add(routeData);
                    seqNo += 1;
                }
            }

            foreach (RouteData r in postCodeList)
            {
                (bool isValid, decimal latitude, decimal longitude) = FetchGeoLocation(r.PostCode);

                if (!isValid) continue;

                r.Valid = true;
                r.Latitude = latitude;
                r.Longitude = longitude;
            }

            // now calculate distances

            RouteData lastData = null;
            foreach (RouteData r in postCodeList)
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
                return (true, decimal.Parse(code["latitude"]?.ToString() ?? string.Empty), decimal.Parse(code["longitude"].ToString()));
            }

            return (false, 0, 0);
        }

        public static string Simulate(IDictionary<string, string> query)
        {
            Console.WriteLine("In route simulation module.");
            foreach (string date in CreateFutureDates())
            {
                // For n dates in advance
                JObject sqlDepots = JObject.Parse(TableName.Get("depots", null));

                foreach (JToken depotRow in sqlDepots["result"])
                {
                    // For all depots
                    JObject sqlDrivers = JObject.Parse(TableName.Get("drivers", null));

                    foreach (JToken driverRow in sqlDrivers["result"])
                    {
                        SingleRoute(date, depotRow, driverRow);
                    }

                    break;
                }

                break;
            }

            return Result(232, "route simulated.", null);
        }

        public static void SingleRoute(string date, JToken depotRow, JToken driverRow)
        {
            JArray routeList = new JArray();

            int noStops = RandomNumber(15, 27);
            noStops = 5;

            string[] itemArray = {"fridge", "freezer", "washer", "dryer", "oven", "cooker", "dishwasher"};
            for (int routeSeqNo = 0;
                routeSeqNo <= noStops;
                routeSeqNo += 1)
            {
                JObject route = new JObject
                {
                    {"depotId", depotRow["depotId"]},
                    {"driverId", driverRow["driverId"]},
                    {"routeDate", date},
                    {"routeSeqNo", routeSeqNo},

                    {"addressId", "111"},
                    {"addressPostcode", depotRow["postcode"]},

                    {"routeAction", routeSeqNo == 0 ? "depot" : "delivery"},
                    {"itemType", routeSeqNo == 0 ? "depot" : itemArray[RandomNumber(0, itemArray.Length - 1)]},
                    {"status", "pending"},
                    {
                        "refNo",
                        $"D{depotRow["depotId"]:0#}R{driverRow["driverId"]:0#}S{routeSeqNo:0#}T{date.Substring(date.Length - 2)}"
                    }
                };

                Console.WriteLine($"Route {route}");
            }
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
            const double radius = 6372.8; // km

            double latitudeDelta = ToRadians((double) (lat2 - lat1));
            double longitudeDelta = ToRadians((double) (lon2 - lon1));

            double a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                       Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2) *
                       Math.Cos(ToRadians((double) lat1)) * Math.Cos(ToRadians((double) lat2));
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return (decimal) Math.Round(radius * c, 4); // km
        }


        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }


        private class RouteData
        {
            public int SeqNo { get; set; }
            public string PostCode { get; set; }
            public bool Valid { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public decimal Distance { get; set; }
        }
    }
}