using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using MySql.Data.MySqlClient;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Lambdas
{
    public class RDS
    {
        public APIGatewayProxyResponse depots(APIGatewayProxyRequest request)
        {
            //var userId = request.PathParameters["userId"];
            

            var endpoint = Environment.GetEnvironmentVariable("RDS_ENDPOINT");
            var port = Environment.GetEnvironmentVariable("RDS_PORT");
            var database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            var user = Environment.GetEnvironmentVariable("RDS_USER");
            var password = Environment.GetEnvironmentVariable("RDS_PASSWORD");


            var connectionString =
                $"server={endpoint}; port={port}; database={database}; user={user}; password={password}";

            MySqlConnection db = null;

            var errorMessage = "";
            var errorStatus = false;
            var statusCode = 200;
            var dataList = new List<Info>();


            try
            {
                db = new MySqlConnection(connectionString);

                try
                {
                    db.Open();
                }
                catch (Exception error)
                {
                    errorMessage = $"failed MySQL Open: ${error}";
                    errorStatus = true;
                    statusCode = 520;
                }
            }
            catch (Exception error)
            {
                errorMessage = $"failed MySQLConnection: ${error}";
                errorStatus = true;
                statusCode = 521;
            }


            if (!errorStatus)
                try
                {
                    // Perform database operations

                    var sql = "select depotName, postCode, fleetSize FROM depots";
                    var cmd = new MySqlCommand(sql, db);
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                        dataList.Add(new Info(
                            rdr.GetString("depotName"),
                            rdr.GetString("postCode"),
                            rdr.GetInt32("fleetSize")
                        ));

                    rdr.Close();
                    db.Close();
                }
                catch (Exception error)
                {
                    errorStatus = true;
                    errorMessage = $"SQL error {error}";
                    statusCode = 522;
                }

            var body = "";

            if (errorStatus)
            {
                LambdaLogger.Log($"===> ${errorMessage}");
                body = "{}";
            }
            else
            {
                body = JsonSerializer.Serialize(dataList);
            }

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