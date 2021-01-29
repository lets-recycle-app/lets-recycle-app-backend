using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DatabaseFunctions
{
    class Program
    {
        static void Main(string[] args)
        {
            string requestName = "drivers";
            string requestId = "0";
            
            RequestFarm requestFarm = new RequestFarm(requestName, requestId);
            
            requestFarm.Process();
            
            
            
            /*
            if (database.connect())
            {
                const string sqlText = "select depotId, depotName, postCode, fleetSize from depots";

                if (database.execute(sqlText,"depots"))
                {
                    body = JsonConvert.SerializeObject(database.mySqlReturnData);

                    Console.WriteLine(body);
                }
                else
                {
                    Console.WriteLine(database.mySqlErrorMessage);
                }
            }

            database.close();
            */

            requestFarm.showResponse();
            
            //jsonTest();
        }
        
        static void jsonTest()
        {
            var my_jsondata = new
            {
                Host = "sftp.myhost.gr",
                UserName = "my_username",
                Password = "my_password",
                SourceDir = "/export/zip/mypath/",
                FileName = "my_file.zip"
            };

            //Transform it to Json object
            string json_data = JsonConvert.SerializeObject(my_jsondata,Formatting.Indented);

            //Print the Json object
            //Console.WriteLine(json_data);

            //Parse the json object
            JObject json_object = JObject.Parse(json_data);

            Console.WriteLine(json_object.ToString());

            //Print the parsed Json object
            Console.WriteLine((string)json_object["Host"]);
            Console.WriteLine((string)json_object["UserName"]);
            Console.WriteLine((string)json_object["Password"]);
            Console.WriteLine((string)json_object["SourceDir"]);
            Console.WriteLine((string)json_object["FileName"]);
        }
    }
}