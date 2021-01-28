using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Lambdas
{
    public class Database
    {
        public List<Depots> mySqlReturnData = new List<Depots>();
        public string mySqlErrorMessage;
        public bool mySqlExecuteStatus;
        private readonly string connectionString;
        private MySqlConnection mySqlConnection;
        
        private bool mySqlConnectionStatus;
        

        public Database()
        {
            mySqlConnection = null;
            mySqlErrorMessage = "";
            mySqlConnectionStatus = false;
            mySqlExecuteStatus=false;

            string database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            string endpoint = Environment.GetEnvironmentVariable("RDS_ENDPOINT");
            string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");
            string port = Environment.GetEnvironmentVariable("RDS_PORT");
            string user = Environment.GetEnvironmentVariable("RDS_USER");

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

        public bool execute(string sqlText)
        {
            
            
            mySqlExecuteStatus=true;
            
            try
            {
                
                MySqlCommand sqlCommand = new MySqlCommand(sqlText, mySqlConnection);
                MySqlDataReader reader = sqlCommand.ExecuteReader();


                while (reader.Read())
                        
                    mySqlReturnData.Add(new Depots(
                        reader.GetInt32("depotId"),
                        reader.GetString("depotName"),
                        reader.GetString("postCode"),
                        reader.GetInt32("fleetSize")
                    ));

                mySqlExecuteStatus=true;
                
                reader.Close();
                
            }
            catch (Exception error)
            {
                
                mySqlErrorMessage = $"SQL Error: ${error}";

                return false;
            }

            return mySqlExecuteStatus;
        }

        public void close()
        {
            if (mySqlConnectionStatus && mySqlConnection != null)
            {
                mySqlConnection.Close();
            }
        }
    }
}