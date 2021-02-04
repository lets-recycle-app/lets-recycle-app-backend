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
            var request = new APIGatewayProxyRequest
            {
                Body =
                    "{\"locationType\": \"private property\",\"customerName\": \"Danny Jones\",\"customerEmail\": \"aaa@aa.aa\",\"itemType\": \"washer\",\"houseNo\": \"12\",\"street\": \"Some St\", \"townAddress\": \"Sometown\", \"postcode\": \"AL5 3EJ\",\"notes\": \"lorem ipsum dolor sit amet\"}",
                Path = "/api/collect-confirm",
                HttpMethod = "POST",

                QueryStringParameters = new Dictionary<string, string>
                {
                    {"driverId", "3"}
                }
            };

            Console.WriteLine(Body(request));

            return 0;
        }
    }
}