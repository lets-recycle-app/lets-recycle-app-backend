﻿using System;
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
                Path = "/api/route-simulate",
                //Path = "/api/route-distance",
                HttpMethod = "GET",

                QueryStringParameters = new Dictionary<string, string>
                {
                    {"postcode1", "AL5 3EJ"},
                    {"postcode3", "HA1 4RL"},
                    {"postcode2", "CH43 8TJ"},
                }
            };

            Console.WriteLine(Body(request));

            return 0;
        }
    }
}