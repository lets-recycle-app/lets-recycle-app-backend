using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Route
    {
        private static readonly Random Random = new Random();

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

            for (int routeSeqNo = 0; routeSeqNo <= noStops; routeSeqNo += 1)
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
                    {"refNo", $"D{depotRow["depotId"]:0#}R{driverRow["driverId"]:0#}S{routeSeqNo:0#}T{date.Substring(date.Length - 2)}"}
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
    }
}