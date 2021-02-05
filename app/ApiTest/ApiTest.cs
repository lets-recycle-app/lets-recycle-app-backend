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
            for (int depotId = 1; depotId <= 20; depotId += 1)
            for (int dayNo = 1; dayNo <= 5; dayNo += 1)
            {
                var request = new APIGatewayProxyRequest
                {
                    Body =
                        "{\"locationType\": \"private property\",\"customerName\": \"Danny Jones\",\"customerEmail\": \"aaa@aa.aa\",\"itemType\": \"washer\",\"houseNo\": \"12\",\"street\": \"Some St\", \"townAddress\": \"Sometown\", \"postcode\": \"AL5 3EJ\",\"notes\": \"lorem ipsum dolor sit amet\"}",
                    Path = "/api/route-simulate",
                    //Path = "/api/route-distance",
                    HttpMethod = "GET",

                    QueryStringParameters = new Dictionary<string, string>
                    {
                        {"depotId", depotId.ToString()},
                        {"dayNo", dayNo.ToString()}
                    }
                    
                };

                Console.WriteLine(Body(request));
                
            }

            return 0;
        }
    }
}