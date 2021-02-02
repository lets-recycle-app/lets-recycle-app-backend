using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ApiCore
{
    public static class Main
    {
        private const int Limit = 1000;

        public static JObject Body(string httpMethod, string endPoint)
        {
            (string action, (string, string)[] query) = ProcessEndPointPath(endPoint);

            return httpMethod switch
            {
                "GET" => ApiGet(action, query),
                "POST" => ApiPost(action),
                _ => Result(400, "error, invalid service [{httpMethod}]", 0, null)
            };
        }

        private static JObject ApiGet(string action, (string, string)[] query)
        {
            if (action == "collect-request")
            {
                return Collect.Request();
            }

            Table tableDesc = IsValidTable(action);

            if (tableDesc == null)
            {
                return Result(400, "error, service not supported [GET]", 0, null);
            }

            if (!tableDesc.IsQueryValid(query))
            {
                return Result(403, $"Invalid field name {tableDesc.InvalidField}", 0, null);
            }

            return Database.Execute(ConstructSql(tableDesc));
        }


        private static JObject ApiPost(string action)
        {
            return action switch
            {
                "collect-confirm" => Collect.Confirm(),
                "collect-update" => Collect.Update(),
                "collect-cancel" => Collect.Cancel(),
                _ => Result(400, "error, service not supported [POST]", 0, null)
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

            sqlText += $" limit {Limit}";

            return sqlText;
        }


        private static (string, (string, string)[]) ProcessEndPointPath(string endPoint)
        {
            const int maxQueries = 10;
            const string querySeparator = "?";

            (string, string)[] queryArray = new (string, string)[maxQueries];

            // after filtering for the last ~/api/
            // only process the first subsequent path.
            // the extended paths are available if required. 

            string fullSegment = PathSplit(endPoint)[0].Trim();


            if (!fullSegment.Contains(querySeparator))
            {
                // no queries specified only a single action instruction/tableName
                return (fullSegment, null);
            }


            // split fullSegment into action?query

            string[] split = fullSegment.Split(querySeparator);
            string actionSegment = split[0].Trim();
            string querySegment = split[1].Trim();


            int count = 0;

            if (querySegment.Length > 0)
            {
                string[] splitAndClauses = querySegment.Split('&');

                foreach (var keyValue in splitAndClauses)
                {
                    string[] equalClause = keyValue.Split('=');

                    if (equalClause.Length != 2) continue;
                    queryArray[count].Item1 = equalClause[0].Trim();
                    queryArray[count].Item2 = equalClause[1].Trim();
                    count += 1;
                }
            }

            Array.Resize(ref queryArray, count);

            return (actionSegment, queryArray);
        }

        private static string[] PathSplit(string endPoint)
        {
            // expect the REST endpoint in the form of
            // ~/api/{service}/{options+}
            // only process paths after /api and
            // return an array with the service in index 0
            // and subsequent options from index 1...


            const int maxPaths = 4;
            string[] pathList = new string[maxPaths];

            for (int i = 0; i < maxPaths; i++)
            {
                // initialise so that paths are never null
                pathList[i] = "_none_";
            }


            string[] pathSplit = endPoint.Split('/');

            int pathCount = 0;

            foreach (string thisPathDir in pathSplit)
            {
                if (thisPathDir == "api")
                {
                    // only start reading options from the
                    // /api encountered

                    pathCount = 1;

                    for (int i = 0; i < maxPaths; i++)
                    {
                        pathList[i] = "_none_";
                    }
                }
                else if (pathCount > 0 && pathCount < maxPaths && thisPathDir.Length > 0)
                {
                    pathList[pathCount - 1] = thisPathDir;
                    pathCount += 1;
                }
            }

            return pathList;
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

        public static JObject Result(int status, string message, int count, JArray data)
        {
            data ??= new JArray();
            JObject response = new JObject
            {
                {"status", status},
                {"message", message},
                {"count", count},
                {"limit", Limit},
                {"result", data}
            };

            return response;
        }

        public static JObject Headers()
        {
            return new JObject
            {
                {
                    "headers", new JObject
                    {
                        {"Content-Type", "application/json"},
                        {"Access-Control-Allow-Origin", "*"}
                    }
                }
            };
        }
    }
}