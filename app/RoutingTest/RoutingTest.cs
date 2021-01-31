using System;
using Routing;

namespace RoutingTest
{
    internal static class RoutingTest
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RoutingTest ~/api/{service}/{options+}");
                return 1;
            }

            string apiRoute = args[0];

            apiRoute = "https://f4d5iqwknf.execute-api.eu-west-2.amazonaws.com/api/depots ? depotId=3&fleetSize=15";
            //apiRoute = "https://f4d5iqwknf.execute-api.eu-west-2.amazonaws.com/api/postcodes?postcode=\"CH43 8TJ\"";
            

            Console.WriteLine($"Api Route [{apiRoute}]");

            RouteFarm routeFarm = new RouteFarm("GET", apiRoute);

            routeFarm.ResponseJson.Show();

            return 0;
        }
    }
}