using System;
using Newtonsoft.Json;

namespace DatabaseFunctions
{
    class Program
    {
        static void Main(string[] args)
        {
            Database database = new Database();

            string body = "{}";


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

            Console.WriteLine($"Body: {body}");
        }
    }
}