using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Routing
{
    public class RouteFarm
    {
        public const int MaxRowLimit = 1000;

        public readonly BodyContainer Body;

        public readonly Dictionary<string, string> Headers = new Dictionary<string, string>
        {
            {"Content-Type", "application/json"},
            {"Access-Control-Allow-Origin", "*"}
        };

        private Database _database;

        public RouteFarm(string httpMethod, string endPoint)
        {
            Body = new BodyContainer();

            Console.WriteLine($"Method: {httpMethod}");
            Console.WriteLine($"EndPoint: {endPoint}");

            (string action, (string, string)[] query) = ProcessEndPointPath(endPoint);

            Table tableDesc = CheckIfSqlQuery(action);

            Console.WriteLine($"[{Body.result}]");
            if (tableDesc == null)
            {
                Body.status = 400;
                Body.message = $"error, bad path supplied [{endPoint}]";
                return;
            }

            string sqlText = ConstructSql(tableDesc, query);

            if (sqlText.Length <= 0) return;

            Body.message = "OK";
            Body.status = 200;
            ExecuteSql(sqlText);
        }

        private static Table CheckIfSqlQuery(string action)
        {
            Table tableDesc = action switch
            {
                "depots" => new Table(action, "depotId (int), depotName, postCode, fleetSize (int)"),

                "drivers" => new Table(action,
                    "driverId (int),  depotId (int), driverName, truckSize (int), userName, apiKey"),

                "routes" => new Table(action,
                    "depotId, driverId, routeDate (date), routeSeqNo, addressId, routeAction, itemType, status, refNo"),

                "postcodes" => new Table(action, "postcodeId (int), postcode, latitude (dec), longitude (dec)"),

                _ => null
            };

            return tableDesc;
        }


        private string ConstructSql(Table table, (string, string)[] query)
        {
            string sqlText = "";

            // compare the query string columns to the table definition
            // throw query out if column names do not match

            bool queryOn = false;

            if (query != null && query.Length > 0)
            {
                foreach (var (fieldName, value) in query)
                {
                    if (table.IsField(fieldName))
                    {
                        // valid column found, so add value and activate query 

                        if (!table.SetFieldQuery(fieldName, value))
                        {
                            Body.message = $"Internal error accessing field name {fieldName}";
                            return "";
                        }

                        queryOn = true;
                    }
                    else
                    {
                        Body.message = $"Invalid field name {fieldName}";
                        return "";
                    }
                }
            }

            sqlText += $"select {table.FieldTextString} from {table.TableName}";

            if (queryOn)
            {
                sqlText += " where ";

                int clauseCount = 0;

                foreach (var field in table.AllFields)
                {
                    if (field.QueryActive)
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
            }

            sqlText += $" limit {MaxRowLimit}";

            Body.message = "OK";
            Body.status = 200;

            Console.WriteLine(sqlText);
            return sqlText;
        }


        private void ExecuteSql(string sqlText)
        {
            if (sqlText.Length > 0)
            {
                // valid sql service name found

                _database = new Database();

                if (_database.Connect())
                {
                    if (_database.Execute(sqlText))
                    {
                        Body.result = _database.MySqlReturnData;
                        Body.count = _database.MySqlRowsReturned;

                        if (!_database.MySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            Body.status = 500;
                            Body.message = _database.MySqlErrorMessage;
                        }
                        else if (_database.MySqlExecuteStatus)
                        {
                            // database statement performed successfully
                            Body.status = 200;
                            Body.message = "OK";
                        }
                        else
                        {
                            // connected ok, but the database statement failed
                            Body.status = 550;
                            Body.message = _database.MySqlErrorMessage;
                        }
                    }
                }

                _database.Close();
            }
            else
            {
                // bad service requested - client error
                Body.status = 400;
                Body.message = "no service requested";
            }
        }

        private (string, (string, string)[]) ProcessEndPointPath(string endPoint)
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

                    if (equalClause.Length == 2)
                    {
                        queryArray[count].Item1 = equalClause[0].Trim();
                        queryArray[count].Item2 = equalClause[1].Trim();
                        count += 1;
                    }
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

        public void ShowResponseMessage()
        {
            Console.WriteLine(JsonConvert.SerializeObject(Headers, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(Body, Formatting.Indented));
        }

        public class BodyContainer
        {
            public int count;
            public int limit;
            public string message;
            public object result;
            public int status;

            public BodyContainer()
            {
                status = 500;
                message = "internal error";
                result = new string[0];
                count = 0;
                limit = MaxRowLimit;
            }
        }
    }
}