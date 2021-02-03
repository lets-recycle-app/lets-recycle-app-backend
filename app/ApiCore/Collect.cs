using System;
using System.Collections.Generic;
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
                ? Result(200, "no collection dates available", 0, dates)
                : Result(200, $"{dates.Count} collection dates available", dates.Count, dates);
        }

        public static string Confirm()
        {
            /*
            JObject admins = Database.GetSqlSelect("select * from admins");

            admins["status"] = 201;
            admins["message"] = "confirm created!";
            admins["count"] = admins.Count;
            admins["result"] = new JArray();

            //dynamic depotObj = JObject.Parse(admins.ToString());

            return admins.ToString();
            */
            return "";
        }

        public static string Update()
        {
            return Result(201, "collection update", 0, null);
        }

        public static string Cancel()
        {
            Console.WriteLine("Collection Cancel");
            return Result(201, "collection cancelled", 0, null);
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