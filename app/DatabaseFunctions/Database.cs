using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DatabaseFunctions
{

    public class Database
    {
        private readonly string connectionString;
        private MySqlConnection mySqlConnection;

        private bool mySqlConnectionStatus;
        public string mySqlErrorMessage;
        public bool mySqlExecuteStatus;
        public string mySqlReturnData;


        public Database()
        {
            mySqlConnection = null;
            mySqlErrorMessage = "";
            mySqlConnectionStatus = false;
            mySqlExecuteStatus = false;
            mySqlReturnData = "";

            string endpoint = Environment.GetEnvironmentVariable("RDS_ENDPOINT");
            string port = Environment.GetEnvironmentVariable("RDS_PORT");
            string database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            string user = Environment.GetEnvironmentVariable("RDS_USER");
            string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");


            connectionString =
                $"server={endpoint}; port={port}; database={database}; user={user}; password={password}";
        }

        public bool connect()
        {
            try
            {
                mySqlConnection = new MySqlConnection(connectionString);

                try
                {
                    mySqlConnection.Open();
                    mySqlConnectionStatus = true;
                }
                catch (Exception error)
                {
                    mySqlErrorMessage = $"failed MySQL Open: ${error}";
                }
            }
            catch (Exception error)
            {
                mySqlErrorMessage = $"failed MySQLConnection: ${error}";
            }

            return mySqlConnectionStatus;
        }

        public bool execute(string sqlText, string table)
        {
            mySqlExecuteStatus = true;
            
            if (table == "depots")
            {
                List<Depots> dataList = new List<Depots>();
                try
                {
                    MySqlCommand sqlCommand = new MySqlCommand(sqlText, mySqlConnection);
                    MySqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        
                        Depots obj;
                        obj = new Depots();

                        obj.depotId = reader.GetInt32("depotId");
                        obj.depotName = reader.GetString("depotName");
                        obj.postCode = reader.GetString("postCode");
                        obj.fleetSize = reader.GetInt32("fleetSize");
                        dataList.Add(obj);
                    }

                    reader.Close();
                    mySqlExecuteStatus = true;
                    mySqlReturnData=JsonConvert.SerializeObject(dataList);
                    
                    return mySqlExecuteStatus;
                }
                catch (Exception error)
                {
                    mySqlErrorMessage = $"SQL Error: ${error}";

                    return false;
                }
            }
            
            if (table == "drivers")
            {
                List<Drivers> dataList = new List<Drivers>();
                
                try
                {
                    MySqlCommand sqlCommand = new MySqlCommand(sqlText, mySqlConnection);
                    MySqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        Drivers obj;
                        obj = new Drivers();

                        obj.driverId = reader.GetInt32("driverId");
                        obj.depotId = reader.GetInt32("depotId");
                        obj.driverName = reader.GetString("driverName");
                        obj.truckSize = reader.GetInt32("truckSize");
                        obj.userName = reader.GetString("userName");
                        obj.apiKey = reader.GetString("apiKey");
                        dataList.Add(obj);
                    }
                    
                    reader.Close();
                    mySqlExecuteStatus = true;
                    mySqlReturnData=JsonConvert.SerializeObject(dataList);
                    
                    return mySqlExecuteStatus;
                }
                catch (Exception error)
                {
                    mySqlErrorMessage = $"SQL Error: ${error}";

                    return false;
                }
            }

            return false;
        }

        public void close()
        {
            if (mySqlConnectionStatus && mySqlConnection != null) mySqlConnection.Close();
        }
    }
}