using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Routing
{
    public class RouteFarm
    {
        private Database _database;
        
        public string Body { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } 

        public RouteFarm(string httpMethod, string endPoint)
        {
            Console.WriteLine($"Method: {httpMethod}");
            Console.WriteLine($"EndPoint: {endPoint}");

            CreateResponseMessage();

            (string action, (string, string)[] query) = ProcessEndPointPath(endPoint);

            Table tableDesc = CheckIfSqlQuery(action);

            if (tableDesc != null)
            {
                (string sqlText, string sqlError) = ConstructSql(tableDesc, query);

                if (sqlText.Length == 0)
                {
                    Console.WriteLine($"SqlError [{sqlError}]");
                }

                ExecuteSql(sqlText);
            }
            else
            {
                Console.WriteLine($"Error: Bad route supplied [{endPoint}]");
                Console.WriteLine("Usage: ~/api/{service}/{action+}");
            }
        }

        private Table CheckIfSqlQuery(string action)
        {
            Table tableDesc = null;

            if (action == "depots")
            {
                tableDesc = new Table(action, "depotId (int), depotName, postCode, fleetSize (int)");
            }
            else if (action == "drivers")
            {
                tableDesc = new Table(action,
                    "driverId (int),  depotId (int), driverName, truckSize (int), userName, apiKey");
            }
            else if (action == "routes")
            {
                tableDesc = new Table(action,
                    "depotId, driverId, routeDate, routeSeqNo, addressId, routeAction, itemType, status, refNo");
            }
            else if (action == "postcodes")
            {
                tableDesc = new Table(action, "postcodeId (int), postcode, latitude (dec), longitude (dec)");
            }

            return tableDesc;
        }


        private static (string, string) ConstructSql(Table table, (string, string)[] query)
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
                            return ("", $"Internal error accessing field name {fieldName}");
                        }

                        queryOn = true;
                    }
                    else
                    {
                        return ("", $"Invalid field name {fieldName}");
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

                        sqlText += $"{field.Name} = {field.QueryValue}";
                        clauseCount += 1;
                    }
                }
            }

            return (sqlText, "Ok");
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
                        Body = _database.MySqlReturnData;


                        if (!_database.MySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            StatusCode = 500;
                        }
                        else if (_database.MySqlExecuteStatus)
                        {
                            // database statement performed successfully
                            StatusCode = 200;
                        }
                        else
                        {
                            // connected ok, but the database statement failed
                            StatusCode = 550;
                        }
                    }
                }

                _database.Close();
            }
            else
            {
                // bad service requested - client error
                StatusCode = 400;
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

        private string[] PathSplit(string endPoint)
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
        
        private void CreateResponseMessage()
        {
            Body = "";
            StatusCode = 500;

            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}
            };
        }
        
        public void ShowResponseMessage()
        {
            Console.WriteLine(JsonConvert.SerializeObject(Headers, Formatting.Indented));
            Console.WriteLine(Body);
            Console.WriteLine($"Status: {StatusCode}");
        }

    }
}