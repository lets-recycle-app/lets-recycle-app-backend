using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Collect
    {
        private static readonly Random Random = new Random();

        public static string Request()
        {
            JArray dates = GetCollectionDates();

            return dates.Count == 0
                ? Result(200, "no collection dates available", dates)
                : Result(200, $"{dates.Count} collection dates available", dates);
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

            JObject postInfo = JObject.Parse(TableName.Post("addresses", newAddress.ToObject<Dictionary<string, string>>()));
            
            if (postInfo["status"].ToString() != "200")
            {
                return postInfo.ToString();
            }
            
            try
            {
                Console.WriteLine(postInfo);
                
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