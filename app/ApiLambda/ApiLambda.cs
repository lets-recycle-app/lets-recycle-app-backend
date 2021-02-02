using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ApiLambda
{
    public class ApiLambda
    {
        public APIGatewayProxyResponse Api(APIGatewayProxyRequest request)
        {
            
            
            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"},
                {"Access-Control-Allow-Methods", "DELETE, POST, GET, OPTIONS"},
                {
                    "Access-Control-Allow-Headers",
                    "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With"
                },
                {"Access-Control-Max-Age", "86400"},
                {"X-Requested-With", "*"}
            };

            JObject body = Body(request.HttpMethod, FullEndpoint(request));
            int statusCode = int.Parse(body["status"].ToString());

            Console.WriteLine($"POST {request.Body}");
            Console.WriteLine($"DEBUG {request.Resource}");
            
            return new APIGatewayProxyResponse
            {
                //Headers = Headers().ToObject<Dictionary<string, string>>(),
                Headers = headers,
                Body = body.ToString(),
                StatusCode = statusCode
                
            };
        }

        private static string FullEndpoint(APIGatewayProxyRequest request)
        {
            // The lambda proxy handler extracts the query string from 
            // the endpoint URL. Recreate the the full http path to ensure
            // that this lambda interface utilises the identical logic 
            // as the local test environment.

            string fullEndpoint = request.Path.Replace("%20", "");

            if (request.QueryStringParameters == null) return fullEndpoint;

            int count = 0;

            foreach (var (key, value) in request.QueryStringParameters)
            {
                if (count > 0)
                {
                    fullEndpoint += "&";
                }
                else
                {
                    fullEndpoint += "?";
                }

                fullEndpoint += $"{key}={value}";
                count += 1;
            }

            return fullEndpoint;
        }
    }
}