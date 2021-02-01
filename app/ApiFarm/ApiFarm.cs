using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;

namespace ApiFarm
{
    public class ApiFarm
    {
        private const int Limit = 1000;
        private readonly Collection _collect;

        public readonly Dictionary<string, string> Headers = new Dictionary<string, string>
        {
            {"Content-Type", "application/json"},
            {"Access-Control-Allow-Origin", "*"}
        };

        private Database _database;


        public ApiFarm(string httpMethod, string endPoint)
        {
            _collect = new Collection(this);

            Console.WriteLine($"Method: {httpMethod}");
            Console.WriteLine($"EndPoint: {endPoint}");

            switch (httpMethod)
            {
                case "GET":
                    ApiGet(endPoint);
                    break;
                case "POST":
                    ApiPost(endPoint);
                    break;
                default:
                    ApiStatus(400, $"error, invalid method [{httpMethod}]");
                    break;
            }
        }

        public int Count { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
        public int Status { get; private set; }

        private void ApiGet(string endPoint)
        {
            (string action, (string, string)[] query) = ProcessEndPointPath(endPoint);

            if (action == "collect-request")
            {
                _collect.Request();
                return;
            }

            Table tableDesc = IsValidTable(action);

            if (tableDesc == null)
            {
                ApiStatus(400, "error, invalid service [GET]");
                return;
            }

            string sqlText = ConstructSql(tableDesc, query);

            if (sqlText.Length <= 0) return;

            Message = "OK";
            Status = 200;
            ExecuteSql(sqlText);
        }


        private void ApiPost(string endPoint)
        {
            (string action, (string, string)[] options) = ProcessEndPointPath(endPoint);

            switch (action)
            {
                case "collect-confirm":
                    Console.WriteLine(options);
                    _collect.Confirm();
                    break;
                case "collect-update":
                    _collect.Update();
                    break;
                case "collect-cancel":
                    _collect.Cancel();
                    break;
                default:
                    ApiStatus(400, "error, invalid service [POST]");
                    break;
            }
        }

        public void ApiStatus(int statusCode, string message)
        {
            Status = statusCode;
            Message = message;
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
                            Message = $"Internal error accessing field name {fieldName}";
                            return "";
                        }

                        queryOn = true;
                    }
                    else
                    {
                        Message = $"Invalid field name {fieldName}";
                        return "";
                    }
                }
            }

            sqlText += $"select {table.FieldTextString} from {table.TableName}";

            if (queryOn)
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

            Message = "OK";
            Status = 200;

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
                        Result = _database.MySqlReturnData;
                        Count = _database.MySqlRowsReturned;

                        if (!_database.MySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            Status = 500;
                            Message = _database.MySqlErrorMessage;
                        }
                        else if (_database.MySqlExecuteStatus)
                        {
                            // database statement performed successfully
                            Status = 200;
                            Message = "OK";
                        }
                        else
                        {
                            // connected ok, but the database statement failed
                            Status = 550;
                            Message = _database.MySqlErrorMessage;
                        }
                    }
                }

                _database.Close();
            }
            else
            {
                // bad service requested - client error
                Status = 400;
                Message = "no service requested";
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

        public void ShowResponseMessage()
        {
            Console.WriteLine(JsonConvert.SerializeObject(Headers, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(GetResponseBody(), Formatting.Indented));
        }

        public dynamic GetResponseBody()
        {
            dynamic response = new ExpandoObject();
            response.status = Status;
            response.message = Message;
            response.count = Count;
            response.limit = Limit;
            response.result = Result;

            return response;
        }
    }
}