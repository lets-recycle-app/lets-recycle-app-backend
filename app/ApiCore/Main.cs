using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace ApiCore
{
    public static class Main
    {
        private const int DataRowReturnLimit = 1000;
        private static int ApiStatusCode { get; set; }

        public static string Body(APIGatewayProxyRequest request)
        {
            string action = request.Path.Replace("/api/", "").Replace("%20", "");

            IDictionary<string, string> query = request.QueryStringParameters;
            ApiStatusCode = 210;

            return request.HttpMethod switch
            {
                "GET" => ApiGet(action, query),
                "POST" => ApiPost(action),
                _ => Result(211, "error, invalid http method [{httpMethod}]", 0, null)
            };
        }

        private static string ApiGet(string action, IDictionary<string, string> query)
        {
            if (action == "collect-request")
            {
                return Collect.Request();
            }

            Table tableDesc = IsValidTable(action);

            if (tableDesc == null)
            {
                return Result(212, "error, service not supported [GET]", 0, null);
            }

            if (!tableDesc.IsQueryValid(query))
            {
                return Result(213, $"invalid field name <{tableDesc.InvalidField}>", 0, null);
            }

            return Database.GetSqlSelect(ConstructSql(tableDesc));
        }


        private static string ApiPost(string action)
        {
            return action switch
            {
                "collect-confirm" => Collect.Confirm(),
                "collect-update" => Collect.Update(),
                "collect-cancel" => Collect.Cancel(),
                _ => Result(214, "error, service not supported [POST]", 0, null)
            };
        }

        private static string ConstructSql(Table table)
        {
            string sqlText = "";

            sqlText += $"select {table.FieldTextString} from {table.TableName}";

            if (table.QueryActive)
            {
                sqlText += " where ";

                int clauseCount = 0;

                foreach (var field in table.AllFields.Where(field => field.QueryActive))
                {
                    if (clauseCount >= 1)
                    {
                        sqlText += " and ";
                    }

                    if (field.FieldType == "date")
                    {
                        sqlText += $"date_format({field.Name},'%Y-%m-%d') = '{field.QueryValue}'";
                    }
                    else
                    {
                        sqlText += $"{field.Name} = {field.QueryValue}";
                    }

                    clauseCount += 1;
                }
            }

            sqlText += $" limit {DataRowReturnLimit}";

            return sqlText;
        }

        private static Table IsValidTable(string action)
        {
            Table tableDesc = action switch
            {
                "addresses" => new Table(action,
                    "addressId (int), postcode, customerName, customerEmail, locationType, fullAddress, houseNo, street, townAddress, notes"),

                "admins" => new Table(action,
                    "adminId (int),  adminName, userName, apiKey"),

                "depots" => new Table(action, "depotId (int), depotName, postcode, fleetSize (int)"),

                "drivers" => new Table(action,
                    "driverId (int),  depotId (int), driverName, truckSize (int), userName, apiKey"),

                "postcodes" => new Table(action, "postcodeId (int), postcode, latitude (dec), longitude (dec)"),

                "routes" => new Table(action,
                    "depotId (int), driverId (int), routeDate (date), routeSeqNo, addressId (int), addressPostcode, routeAction, itemType, status, refNo"),

                _ => null
            };

            return tableDesc;
        }

        public static string Result(int status, string message, int count, JArray data)
        {
            data ??= new JArray();
            ApiStatusCode = status;

            JObject response = new JObject
            {
                {"status", status},
                {"message", message},
                {"count", count}
            };

            if (count >= DataRowReturnLimit)
            {
                response["limit"] = DataRowReturnLimit;
            }

            response["result"] = data;

            return response.ToString();
        }

        public static IDictionary<string, string> Headers()
        {
            return new Dictionary<string, string>
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
        }

        public static int StatusCode()
        {
            return ApiStatusCode;
        }
    }
}