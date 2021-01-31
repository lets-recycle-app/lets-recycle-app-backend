using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Routing;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaApi
{
    public class LambdaApi
    {
        public APIGatewayProxyResponse ApiGet(APIGatewayProxyRequest request)
        {
            string apiRoute = request.PathParameters["api"];
            
            RouteFarm routeFarm = new RouteFarm(request.HttpMethod, FullEndpoint(request));

            return new APIGatewayProxyResponse
            {
                Body = routeFarm.ResponseJson.Body,
                Headers = routeFarm.ResponseJson.Headers,
                StatusCode = routeFarm.ResponseJson.StatusCode
            };
        }

        private string FullEndpoint(APIGatewayProxyRequest request)
        {
            string fullEndpoint = request.Path.Replace("%20","");

            int count = 0;

            if (request.QueryStringParameters != null)
            {
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
            }

            return fullEndpoint;
        }

        public APIGatewayProxyResponse ApiPost(APIGatewayProxyRequest request)
        {
            string apiRoute = request.PathParameters["api"];

            Console.WriteLine("-------------- P O S T ------------------");
            Console.WriteLine($"Api Post Route [{apiRoute}]");
            Console.WriteLine($"Body {request.Body}");


            RouteFarm routeFarm = new RouteFarm("GET",apiRoute);

            return new APIGatewayProxyResponse
            {
                Body = routeFarm.ResponseJson.Body,
                Headers = routeFarm.ResponseJson.Headers,
                StatusCode = routeFarm.ResponseJson.StatusCode
            };
        }
    }
}