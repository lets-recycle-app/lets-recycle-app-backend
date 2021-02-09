using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using static ApiCore.Main;

namespace ApiTest
{
    internal static class ApiTest
    {
        private static int Main()
        {
            RunMap();
            //GenerateRoutes();

            return 0;
        }

        static void GenerateRoutes()
        {
            for (int depotId = 1; depotId <= 1; depotId += 1)
            for (int dayNo = 1; dayNo <= 5; dayNo += 1)
            {
                var request = new APIGatewayProxyRequest
                {
                    Body = "",
                    Path = "/api/route-simulate",
                    HttpMethod = "GET",

                    QueryStringParameters = new Dictionary<string, string>
                    {
                        {"depotId", depotId.ToString()},
                        {"dayNo", dayNo.ToString()}
                    }
                };

                Console.WriteLine(Body(request));
            }
        }

        static void RunMap()
        {
            var request = new APIGatewayProxyRequest
            {
                Body =
                    "{\"locationType\": \"private property\",\"customerName\": \"Danny Jones\",\"customerEmail\": \"danny123@gmail.com.aa\",\"itemType\": \"washer\",\"houseNo\": \"8\",\"street\": \"Station Road\", \"townAddress\": \"Stockport\", \"postcode\": \"SK4 1NU\",\"notes\": \" \"}",
                Path = "/api/route-marker",
                HttpMethod = "GET",

                QueryStringParameters = new Dictionary<string, string>
                {
                    {"depotId", "3"},
                    {"driverId", "47"},
                    {"dayNo", "1"}
                }
            };


            Console.WriteLine(Body(request));
        }
    }
}