using System;
using System.Collections.Generic;
using System.Dynamic;
using MySql.Data.MySqlClient;

namespace ApiFarm
{
    public class Database
    {
        private readonly string _connectionString;
        private MySqlConnection _mySqlConnection;

        public bool MySqlConnectionStatus;
        public string MySqlErrorMessage;
        public bool MySqlExecuteStatus;
        public object MySqlReturnData;
        public int MySqlRowsReturned;


        public Database()
        {
            _mySqlConnection = null;
            MySqlConnectionStatus = false;
            MySqlExecuteStatus = false;
            MySqlErrorMessage = "";
            MySqlReturnData = "";
            MySqlRowsReturned = 0;

            string endpoint = Environment.GetEnvironmentVariable("RDS_ENDPOINT");
            string port = Environment.GetEnvironmentVariable("RDS_PORT");
            string database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            string user = Environment.GetEnvironmentVariable("RDS_USER");
            string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");


            _connectionString =
                $"server={endpoint}; port={port}; database={database}; user={user}; password={password}";
        }

        public bool Connect()
        {
            try
            {
                _mySqlConnection = new MySqlConnection(_connectionString);

                try
                {
                    _mySqlConnection.Open();
                    MySqlConnectionStatus = true;
                }
                catch (Exception error)
                {
                    MySqlErrorMessage = $"failed MySQL Open: ${error}";
                }
            }
            catch (Exception error)
            {
                MySqlErrorMessage = $"failed MySQLConnection: ${error}";
            }

            return MySqlConnectionStatus;
        }

        public bool Execute(string sqlText)
        {
            MySqlCommand sqlCommand = new MySqlCommand(sqlText, _mySqlConnection);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            try
            {
                var tableData = new List<dynamic>();

                while (reader.Read())
                {
                    var row = new ExpandoObject();
                    var fields = (IDictionary<string, object>) row;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetFieldType(i) == typeof(int))
                        {
                            fields[reader.GetName(i)] = reader.GetInt32(i);
                        }
                        else
                        if (reader.GetFieldType(i) == typeof(decimal))
                        {
                            fields[reader.GetName(i)] = reader.GetDecimal(i);
                        }
                        else
                        {
                            fields[reader.GetName(i)] = reader.GetString(i);
                        }
                    }

                    tableData.Add(row);
                }

                MySqlExecuteStatus = true;
                MySqlReturnData = tableData;
                MySqlRowsReturned = tableData.Count;
            }
            catch (Exception error)
            {
                MySqlErrorMessage = $"SQL Error: ${error}";
            }

            return MySqlExecuteStatus;
        }

        public void Close()
        {
            if (MySqlConnectionStatus) _mySqlConnection?.Close();
        }
    }
}