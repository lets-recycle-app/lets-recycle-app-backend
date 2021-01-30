using System;
using System.Collections.Generic;

namespace Routing
{
    public class RouteFarm
    {
        private readonly List<string> _routeOptions = new List<string>();
        private Database _database;

        public RouteFarm(string apiRoute)
        {
            ResponseJson = new Response();

            if (ParseRouteOptions(apiRoute))
            {
                Process();
            }
        }

        public Response ResponseJson { get; }

        private void Process()
        {
            string sqlText = CheckIfSql();

            if (sqlText.Length > 0)
            {
                // valid sql service name found

                _database = new Database();

                if (_database.Connect())
                {
                    if (_database.Execute(sqlText))
                    {
                        ResponseJson.Body = _database.MySqlReturnData;


                        if (!_database.MySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            ResponseJson.StatusCode = 500;
                        }
                        else if (_database.MySqlExecuteStatus)
                        {
                            // database statement performed successfully
                            ResponseJson.StatusCode = 200;
                        }
                        else
                        {
                            // connected ok, but the database statement failed
                            ResponseJson.StatusCode = 550;
                        }
                    }
                }

                _database.Close();
            }
            else
            {
                // bad service requested - client error
                ResponseJson.StatusCode = 400;
            }
        }

        private bool ParseRouteOptions(string apiRoute)
        {
            // expect a route in the form of ~/api/{service}/{options+}
            // store all options after /api/

            string[] pathSplits = apiRoute.Split('/');

            int optionCount = 0;

            foreach (string thisPathDir in pathSplits)
            {
                switch (optionCount)
                {
                    case 0 when thisPathDir == "api":
                        optionCount = 1;

                        break;
                    default:
                    {
                        if (optionCount > 0)
                        {
                            _routeOptions.Add(thisPathDir);
                            optionCount += 1;
                        }

                        break;
                    }
                }
            }

            if (optionCount != 0) return true;

            Console.WriteLine("Error: Bad route [~/api/{service}/{options+}].");
            return false;
        }

        string CheckIfSql()
        {
            if (_routeOptions.Count == 0)
            {
                return "";
            }

            string table = _routeOptions[0];
            string idString = "";

            if (_routeOptions.Count > 1)
            {
                idString = _routeOptions[1];
            }


            if (!int.TryParse(idString, out int id))
            {
                id = 0;
            }

            string sqlText = table switch
            {
                "depots" =>
                    "select depotId, depotName, postCode, fleetSize from depots",

                "drivers" =>
                    "select driverId, depotId, driverName, truckSize, userName, apiKey from drivers",

                "admins" =>
                    "select adminId from admins",

                _ => ""
            };

            if (id != 0)
            {
                sqlText += table switch
                {
                    "depots" =>
                        $" where depotId = {id}",

                    "drivers" =>
                        $" where driverId = {id}",

                    "admins" =>
                        $" where adminId = {id}",

                    _ => ""
                };
            }

            return sqlText;
        }
    }
}