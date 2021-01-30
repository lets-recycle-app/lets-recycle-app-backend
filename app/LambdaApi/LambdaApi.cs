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
            
            Console.WriteLine("-------------- G E T ------------------");
            Console.WriteLine($"Api Get Route [{apiRoute}]");
            Console.WriteLine($"Body {request.Body}");
            

            RouteFarm routeFarm = new RouteFarm(apiRoute);

            return new APIGatewayProxyResponse
            {
                Body = routeFarm.ResponseJson.Body,
                Headers = routeFarm.ResponseJson.Headers,
                StatusCode = routeFarm.ResponseJson.StatusCode
            };
        }
        
        public APIGatewayProxyResponse ApiPost(APIGatewayProxyRequest request)
        {
            string apiRoute = request.PathParameters["api"];
            
            Console.WriteLine("-------------- P O S T ------------------");
            Console.WriteLine($"Api Post Route [{apiRoute}]");
            Console.WriteLine($"Body {request.Body}");

            
            RouteFarm routeFarm = new RouteFarm(apiRoute);

            return new APIGatewayProxyResponse
            {
                Body = routeFarm.ResponseJson.Body,
                Headers = routeFarm.ResponseJson.Headers,
                StatusCode = routeFarm.ResponseJson.StatusCode
            };
        }
    }
}