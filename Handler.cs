using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using MySql.Data.MySqlClient;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        public APIGatewayProxyResponse GetData(APIGatewayProxyRequest request)
        {
            //var userId = request.PathParameters["userId"];


            var server = Environment.GetEnvironmentVariable("DATABASE_INSTANCE");
            var port = Environment.GetEnvironmentVariable("DATABASE_PORT");
            var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
            var user = Environment.GetEnvironmentVariable("DATABASE_USER");
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
            

            var connectionString = string.Format(
                "server={0}; port={1}; database={2}; user={3}; password={4}",
                server, port, database, user, password);

            MySqlConnection db=null;
            
            var errorMessage = "";
            bool errorStatus = false;
            int statusCode = 200;
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
            {
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
                
            }

            string body="";
            
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

        public APIGatewayProxyResponse SaveData(APIGatewayProxyRequest request)
        {
            var info = JsonSerializer.Deserialize<Info>(request.Body);

            return new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"Content-Type", "application/json"},
                    {"Access-Control-Allow-Origin", "*"}
                },
                Body = info.DepotName,
                StatusCode = 200
            };
        }
    }
}