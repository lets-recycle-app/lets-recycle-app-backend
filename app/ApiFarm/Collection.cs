using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiFarm
{
    public class Collection
    {
        private readonly ApiFarm api;

        public Collection(ApiFarm _api)
        {
            api = _api;
        }

        public void Request()
        {
            Console.WriteLine("Collection Request");

            DateTime currentDay = DateTime.Now.Date;

            string[] dateList =
            {
                "2021-02-02", "2021-02-03", "2021-02-04"
            };

            dynamic stuff =
                JObject.Parse("{ 'Name': 'Jon Smith', 'Address': { 'City': 'New York', 'State': 'NY' }, 'Age': 42 }");

            string name = stuff.Name;
            string address = stuff.Address.City;
            Console.WriteLine($"JSON {name} {address}");

            var response = new {result = 108, status = "All good."};

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            /*
            for (int i = 1; i <= 7; i += 1)
            {
                Console.WriteLine(currentDay.AddDays(i));
            }
            */
            api.ApiStatus(200, "Send request.");
        }

        public void Confirm()
        {
            Console.WriteLine("Collection Confirm");

            api.ApiStatus(201, "Created");
        }

        public void Update()
        {
            Console.WriteLine("Collection Update");

            api.ApiStatus(202, "Updated");
        }

        public void Cancel()
        {
            Console.WriteLine("Collection Cancel");
            api.ApiStatus(203, "Deleted");
        }
    }
}