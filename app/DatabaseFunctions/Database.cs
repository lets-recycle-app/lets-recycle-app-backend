using System;
using System.Collections.Generic;
using System.Dynamic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DatabaseFunctions
{
    public class Database
    {
        private readonly string connectionString;
        private MySqlConnection mySqlConnection;

        public bool mySqlConnectionStatus;
        public string mySqlErrorMessage;
        public bool mySqlExecuteStatus;
        public string mySqlReturnData;


        public Database()
        {
            mySqlConnection = null;
            mySqlConnectionStatus = false;
            mySqlExecuteStatus = false;
            mySqlErrorMessage = "";
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

        public bool execute(string sqlText)
        {
            MySqlCommand sqlCommand = new MySqlCommand(sqlText, mySqlConnection);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            try
            {
                var list = new List<dynamic>();

                while (reader.Read())
                {
                    var obj = new ExpandoObject();
                    var d = obj as IDictionary<string, object>;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetFieldType(i) == typeof(int))
                        {
                            d[reader.GetName(i)] = reader.GetInt32(i);
                        }
                        else
                        {
                            d[reader.GetName(i)] = reader.GetString(i);
                        }
                    }

                    list.Add(obj);
                }

                mySqlExecuteStatus = true;
                mySqlReturnData = JsonConvert.SerializeObject(list, Formatting.Indented);
            }
            catch (Exception error)
            {
                mySqlErrorMessage = $"SQL Error: ${error}";
            }

            return mySqlExecuteStatus;
        }

        public void close()
        {
            if (mySqlConnectionStatus && mySqlConnection != null) mySqlConnection.Close();
        }
    }
}