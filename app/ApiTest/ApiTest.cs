using System;

namespace ApiTest
{
    internal static class RoutingTest
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ApiTest ~/api/{service}?{key}={value}");
                return 1;
            }

            string endPoint = args[0];


            //endPoint = "https://f4d7ipwknd.execute-api.eu-west-2.amazonaws.com/api/depots?depotId=1";
            //endPoint = "/dev/api/postcodes? postcode = CH43 8TJ";

            //endPoint = "/dev/api/collect-request";
            endPoint = "/dev/api/depots";


            Console.WriteLine($"Api Route [{endPoint}]");

            ApiFarm.ApiFarm apiFarm = new ApiFarm.ApiFarm("GET", endPoint);

            apiFarm.ShowResponseMessage();

            return 0;
        }
    }
}