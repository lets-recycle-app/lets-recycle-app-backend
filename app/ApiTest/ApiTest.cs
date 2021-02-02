using System;
using static ApiCore.Main;

namespace ApiTest
{
    internal static class ApiTest
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ApiTest ~/api/{service}?{key}={value}");
                return 1;
            }

            string endPoint = args[0];
            string httpMethod = "GET";


            //endPoint = "https://f4d7ipwknd.execute-api.eu-west-2.amazonaws.com/api/depots?depotId=1";
            //endPoint = "/dev/api/postcodes? postcode = CH43 8TJ";

            
            //endPoint = "/dev/api/depots?depotId=2&fleetSize=13";
            endPoint = "/dev/api/collect-request";
            httpMethod = "GET";
            
            Console.WriteLine(Headers());
            Console.WriteLine(Body(httpMethod, endPoint));

            return 0;
        }
    }
}