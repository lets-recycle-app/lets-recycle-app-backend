using System;

namespace DatabaseFunctions
{
    public class RequestFarm
    {
        private Database _database;

        public RequestFarm(string serviceName, string serviceId)
        {
            ServiceName = serviceName;
            ServiceId = serviceId;

            Response = new Response();
        }

        private string ServiceName { get; }
        private string ServiceId { get; }
        private Response Response { get; }

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
                        Response.Body = _database.mySqlReturnData;


                        if (!_database.mySqlConnectionStatus)
                        {
                            // failed to connect to the database
                            Response.StatusCode = 500;
                        }
                        else if (_database.mySqlExecuteStatus)
                        {
                            // database statement performed successfully
                            Response.StatusCode = 200;
                        }
                        else
                        {
                            // connected ok, but the database statement failed
                            Response.StatusCode = 550;
                        }
                    }
                }

                _database.close();
            }
            else
            {
                // bad service requested - client error
                Response.StatusCode = 400;
            }
        }

        public void ShowResponse()
        {
            Console.WriteLine(Response.Headers);
            Console.WriteLine(Response.Body);
            Console.WriteLine(Response.StatusCode);
        }
    }
}