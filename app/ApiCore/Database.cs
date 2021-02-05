using System;
using System.Net.WebSockets;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace ApiCore
{
    public static class Database
    {
        private static string connectString;

        public static void SetUp()
        {

            string host = Environment.GetEnvironmentVariable("RDS_HOST");
            string port = Environment.GetEnvironmentVariable("RDS_PORT");
            string database = Environment.GetEnvironmentVariable("RDS_DATABASE");
            string user = Environment.GetEnvironmentVariable("RDS_USER");
            string password = Environment.GetEnvironmentVariable("RDS_PASSWORD");

            connectString = $"server={host}; port={port}; database={database}; user={user}; password={password}";

            
        }

        public static string GetSqlSelect(string sqlText)
        {
            using (var _mySql = new MySqlConnection(connectString))
            using (var sqlCommand = new MySqlCommand(sqlText, _mySql))
            {
                _mySql.Open();
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
                                int value = reader[i] as int? ?? default;
                                row.Add(reader.GetName(i), value);
                            }
                            else if (reader.GetFieldType(i) == typeof(decimal))
                            {
                                decimal value = reader[i] as decimal? ?? default;
                                row.Add(reader.GetName(i), value);
                            }
                            else
                            {
                                string value = reader[i] as string;
                                row.Add(reader.GetName(i), value);
                            }
                        }

                        tableData.Add(row);
                    }
                    return Main.Result(tableData.Count > 0 ? 200 : 201, "OK", tableData);
                }
                catch
                {
                    // ignored
                }

                _mySql.Close();
            }

            return Main.Result(502, "error, internal sql failed", null);
        }

        public static string SqlTransaction(string sqlText)
        {
            using (var _mySql = new MySqlConnection(connectString)) 
            using (var sqlCommand = new MySqlCommand(sqlText, _mySql))
            {
                _mySql.Open();

                MySqlTransaction sqlTransaction = _mySql.BeginTransaction();
                JArray txArray = new JArray {new JObject {{"insertId", -1}}};

                try
                {
                    sqlCommand.ExecuteNonQuery();
                    sqlTransaction.Commit();

                    
                    MySqlCommand sqlCommandId = new MySqlCommand("select last_insert_id()", _mySql);
                    MySqlDataReader reader = sqlCommandId.ExecuteReader();

                    reader.Read();

                    txArray[0]["insertId"] = reader.GetInt32(0);

                    reader.Close();

                    return Main.Result(200, "OK", txArray);
                }
                catch
                {
                    try
                    {
                        sqlTransaction.Rollback();
                    }
                    catch
                    {
                        return Main.Result(250, "error, internal sql rollback failed", null);
                    }
                }

                _mySql.Close();
            }

            return Main.Result(251, "error, internal sql transaction failed", null);
        }

        public static JToken SqlSingleRow(string sqlText)
        {
            string selectData = GetSqlSelect(sqlText);

            if (selectData == null) return "{}";

            JToken sqlRow = JObject.Parse(selectData);

            return sqlRow["result"].HasValues ? sqlRow["result"][0] : "{}";
        }

        public static JToken SqlMultiRow(string sqlText)
        {
            string selectData = GetSqlSelect(sqlText);

            if (selectData == null) return "{}";

            JToken sqlRow = JObject.Parse(selectData);

            return sqlRow["result"].HasValues ? sqlRow["result"] : "{[]}";
        }
    }
}