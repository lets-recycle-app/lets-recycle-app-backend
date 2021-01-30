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

            Console.WriteLine($"Api Route [{apiRoute}]");

            RouteFarm routeFarm = new RouteFarm(apiRoute);

            routeFarm.ResponseJson.Show();

            return 0;
        }
    }
}