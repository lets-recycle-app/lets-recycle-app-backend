using System;

namespace Routing
{
    public class RouteFarm
    {
        private Database _database;

        public RouteFarm(string apiRoute)
        {
            
            ServiceName = "serviceName";
            ServiceId = "serviceId";

            ResponseJson = new Response();
        }

        private string ServiceName { get; }
        private string ServiceId { get; }
        private Response ResponseJson { get; }

        public void Process()
        {
            string[] tables = {"depots", "drivers", "admins", "addresses", "routes"};


            if (Array.Exists(tables, element => element == ServiceName))
            {
                //string sqlText = "select depotId, depotName, postCode, fleetSize from depots";
                string sqlText = "select driverId, depotId, driverName, truckSize, userName, apiKey from drivers";

                if (ServiceId != "0")
                {
                    sqlText += $" where depotId = {ServiceId}";
                }

                _database = new Database();

                if (_database.connect())
                {
                    if (_database.execute(sqlText))
                    {
                        ResponseJson.Body = _database.mySqlReturnData;


                        if (!_database.mySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            ResponseJson.StatusCode = 500;
                        }
                        else if (_database.mySqlExecuteStatus)
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

                _database.close();
            }
            else
            {
                // bad service requested - client error
                ResponseJson.StatusCode = 400;
            }
        }

        public void ShowResponse()
        {
            Console.WriteLine(ResponseJson.Headers);
            Console.WriteLine(ResponseJson.Body);
            Console.WriteLine(ResponseJson.StatusCode);
        }
    }
}