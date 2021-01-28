using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using DatabaseFunctions;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaFunctions
{
    public class AwsLambda
    {
        public APIGatewayProxyResponse depots(APIGatewayProxyRequest request)
        {
            //var userId = request.PathParameters["userId"];

            Database database = new Database();

            int statusCode = 200;
            string body = "{}";


            if (database.connect())
            {
                const string sqlText = "select depotId, depotName, postCode, fleetSize from depots";

                if (database.execute(sqlText))
                {
                    body = JsonConvert.SerializeObject(database.mySqlReturnData);
                }
                else
                {
                    LambdaLogger.Log($"===> ${database.mySqlErrorMessage}");
                }
            }

            database.close();

            return new APIGatewayProxyResponse
            {
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"},
                    {"Access-Control-Allow-Origin", "*"}
                },
                StatusCode = statusCode
            };
        }

        /*
        public APIGatewayProxyResponse SaveData(APIGatewayProxyRequest request)
        {
            //var info = JsonSerializer.Deserialize<Info>(request.Body);
            string info = request.Body.ToString();

            return new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"},
                    {"Access-Control-Allow-Origin", "*"}
                },
                Body = info,
                StatusCode = 200
            };
        }
        */
    }
}