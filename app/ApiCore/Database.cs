using System;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace ApiCore
{
    public static class Database
    {
        private static string _connectString;
        private static MySqlConnection _mySql;

        private static bool Connect()
        {
            _mySql = null;

            string host = Environment.GetEnvironmentVariable("RDS_HOST");
            string port = Environment.GetEnvironmentVariable("RDS_PORT");
            string database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            string user = Environment.GetEnvironmentVariable("RDS_USER");
            string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");

            _connectString = $"server={host}; port={port}; database={database}; user={user}; password={password}";


            try
            {
                _mySql = new MySqlConnection(_connectString);

                _mySql.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetSqlSelect(string sqlText)
        {
            if (!Connect())
            {
                return Main.Result(501, "failed to connect to the database", 0, null);
            }

            MySqlCommand sqlCommand = new MySqlCommand(sqlText, _mySql);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            try
            {
                JArray tableData = new JArray();

                while (reader.Read())
                {
                    JObject row = new JObject();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetFieldType(i) == typeof(int))
                        {
                            row.Add(reader.GetName(i), reader.GetInt32(i));
                        }
                        else if (reader.GetFieldType(i) == typeof(decimal))
                        {
                            row.Add(reader.GetName(i), reader.GetDecimal(i));
                        }
                        else
                        {
                            row.Add(reader.GetName(i), reader.GetString(i));
                        }
                    }

                    tableData.Add(row);
                }

                return Main.Result(tableData.Count > 0 ? 200 : 201, "OK", tableData.Count, tableData);
            }
            catch
            {
                // ignored
            }

            _mySql?.Close();

            return Main.Result(502, "error, internal sql failed", 0, null);
        }
    }
}