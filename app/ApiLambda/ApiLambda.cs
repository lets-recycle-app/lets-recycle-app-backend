using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Newtonsoft.Json;
using ApiFarm;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ApiLambda
{
    public class ApiLambda
    {
        public APIGatewayProxyResponse Api(APIGatewayProxyRequest request)
        {
            ApiFarm.ApiFarm apiFarm = new ApiFarm.ApiFarm(request.HttpMethod, FullEndpoint(request));

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(apiFarm.Body),
                Headers = apiFarm.Headers,
                StatusCode = apiFarm.Body.status
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