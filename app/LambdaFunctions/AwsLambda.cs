using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Routing;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace LambdaFunctions
{
    public class AwsLambda
    {
        public APIGatewayProxyResponse Main(APIGatewayProxyRequest request)
        {
            string table = request.PathParameters["table"];
            string tableId = request.PathParameters["tableId"];
            

            Database database = new Database();

            LambdaLogger.Log($"===> Path=[{request.PathParameters}] Method=[{request}]");

            int statusCode = 200;
            string body = "";

            string sqlText = "";
            if (table == "depots")
            {
                sqlText = "select depotId, depotName, postCode, fleetSize from depots";

                if (tableId != "0")
                {
                    sqlText += $" where depotId = {tableId}";
                }


                if (database.connect())
                {
                    if (database.execute(sqlText, "depots"))
                    {
                        body = database.mySqlReturnData;
                    }
                    else
                    {
                        LambdaLogger.Log($"===> ${database.mySqlErrorMessage}");
                    }
                }
            }
            else if (table == "drivers")
            {
                sqlText = "select driverId, depotId, driverName, truckSize, userName, apiKey from drivers";
                if (tableId != "0")
                {
                    sqlText += $" where driverId = {tableId}";
                }

                if (database.connect())
                {
                    if (database.execute(sqlText, "drivers"))
                    {
                        body = database.mySqlReturnData;
                    }
                    else
                    {
                        LambdaLogger.Log($"===> ${database.mySqlErrorMessage}");
                    }
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