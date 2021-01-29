using System;
using Newtonsoft.Json;

namespace DatabaseFunctions
{
    public class RequestFarm
    {
        public RequestFarm(string _requestName, string _requestId)
        {
            requestName = _requestName;
            requestId = _requestId;

            response = new Response();
            Database database;
        }

        public string requestName { get; set; }
        public string requestId { get; set; }
        public Response response { get; }
        private Database database;

        public void Process()
        {
            string[] tables = {"depots", "drivers", "admins", "addresses", "routes"};
            
            
            if (Array.Exists(tables, element => element == requestName))
            {
                string sqlText = "select depotId, depotName, postCode, fleetSize from depots";

                if (requestId != "0")
                {
                    sqlText += $" where depotId = {requestId}";
                }
                
                Console.WriteLine($"Process Sql {sqlText}");
                database = new Database();
                
                if (database.connect())
                {
                    if (database.execute(sqlText, "depots"))
                    {
                        response.Body = database.mySqlReturnData;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Bad Table {requestName}");
            }
        }

        public void showResponse()
        {
            Console.WriteLine(JsonConvert.SerializeObject(response.Headers, Formatting.Indented));
            Console.WriteLine(response.Body);
            Console.WriteLine(response.StatusCode);
        }
    }
}