using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Routing;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaApi
{
    public class LambdaApi
    {
        public APIGatewayProxyResponse Api(APIGatewayProxyRequest request)
        {
            RouteFarm routeFarm = new RouteFarm(request.HttpMethod, FullEndpoint(request));

            return new APIGatewayProxyResponse
            {
                Body = routeFarm.ResponseJson.Body,
                Headers = routeFarm.ResponseJson.Headers,
                StatusCode = routeFarm.ResponseJson.StatusCode
            };
        }

        private static string FullEndpoint(APIGatewayProxyRequest request)
        {
            // lambda proxy handler separates query string from 
            // Rest endpoint. Recreate the the full path to allow
            // this lambda to interface in the same way as the local
            // test environment.
            
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