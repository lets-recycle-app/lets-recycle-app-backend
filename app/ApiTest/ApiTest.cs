using System;
using System.Collections.Generic;
using static ApiCore.Main;
using Amazon.Lambda.APIGatewayEvents;

namespace ApiTest
{
    internal static class ApiTest
    {
        private static int Main()
        {
            var request = new APIGatewayProxyRequest()
            {
                Body = "{\"locationType\": \"private property\",\"customerName\": \"Jane Newman\",\"customerEmail\": \"aaa@aa.aa\",\"itemType\": \"washer\",\"houseNo\": \"12\",\"street\": \"Some St\", \"townAddress\": \"Sometown\", \"postcode\": \"sk1 2lg\",\"notes\": \"lorem ipsum dolor sit amet\"}",
                Path = "/api/depots",
                HttpMethod = "GET",
                
                QueryStringParameters = new Dictionary<string, string>(){
                    {"depotId", "3"}
                }
            };
            
            Console.WriteLine(Body(request));

            return 0;
        }
    }
}