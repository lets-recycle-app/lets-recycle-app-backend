using System;
using System.Collections.Generic;
using System.Dynamic;

namespace ApiFarm
{
    public class Collection
    {
        private readonly ApiFarm _api;
        private readonly Random _random = new Random();

        public Collection(ApiFarm api)
        {
            _api = api;
        }

        public void Request()
        {
            Console.WriteLine("Collection Request");

            _api.Result = GetCollectionDates();
            _api.Count = _api.Result.Count;

            if (_api.Count == 0)
            {
                _api.ApiStatus(201, "no collection dates available");
            }
            else
            {
                _api.ApiStatus(200, $"{_api.Count} collection dates available");
            }
        }

        public void Confirm()
        {
            //dynamic stuff = JObject.Parse("{ 'Name': 'John Smith', 'Address': { 'City': 'New York', 'State': 'NY' }, 'Age': 25 }");
            Console.WriteLine("Collection Confirm");

            _api.ApiStatus(201, "Created");
        }

        public void Update()
        {
            Console.WriteLine("Collection Update");

            _api.ApiStatus(202, "Updated");
        }

        public void Cancel()
        {
            Console.WriteLine("Collection Cancel");
            _api.ApiStatus(203, "Deleted");
        }

        private List<dynamic> GetCollectionDates()
        {
            DateTime currentDay = DateTime.Now.Date;

            List<DateTime> forwardDays = new List<DateTime>();
            List<dynamic> collectDatesList = new List<dynamic>();

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

                dynamic reply = new ExpandoObject();
                {
                    reply.date = date;
                    reply.depotId = depotId;
                    reply.driverId = driverId;
                    reply.routeSeqNo = routeSeqNo;
                    reply.refNo = refNo;
                }

                collectDatesList.Add(reply);
            }

            return collectDatesList;
        }

        private int RandomNumber(int min, int max)
        {
            return _random.Next(min, max + 1);
        }
    }
}