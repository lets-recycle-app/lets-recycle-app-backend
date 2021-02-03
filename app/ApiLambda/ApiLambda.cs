using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using static ApiCore.Main;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ApiLambda
{
    public class ApiLambda
    {
        public APIGatewayProxyResponse Api(APIGatewayProxyRequest request)
        {
            return new APIGatewayProxyResponse
            {
                Headers = Headers(),
                Body = Body(request),
                StatusCode = StatusCode()
            };
        }
    }
}