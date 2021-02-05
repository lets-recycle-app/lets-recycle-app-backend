using System;
using System.Collections.Generic;
using System.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class TableName
    {
        private static readonly Dictionary<string, string> TableDict = new Dictionary<string, string>
        {
            {
                "addresses",
                "addressId (int), postcode, customerName, customerEmail, locationType, houseNo, street, townAddress, notes"
            },
            {
                "admins",
                "adminId (int),  adminName, userName, apiKey"
            },
            {
                "depots",
                "depotId (int), depotName, postcode, fleetSize (int)"
            },
            {
                "drivers",
                "driverId (int),  depotId (int), driverName, truckSize (int), userName, apiKey"
            },
            {
                "postcodes",
                "postcodeId (int), postcode, latitude (dec), longitude (dec)"
            },
            {
                "routes",
                "depotId (int), driverId (int), routeDate (date), routeSeqNo (int), distance (dec), addressId (int), addressPostcode, latitude (dec), longitude (dec), routeAction, itemType, status, refNo"
            }
        };
        
        
        public static string Get(string tableName, IDictionary<string, string> query)
        {
            if (!IsValid(tableName)) return Result(212, "error, service not supported [GET]", null);

            TableFields tableFields = new TableFields(tableName, TableDict[tableName]);

            if (!tableFields.IsFieldListValid(query))
            {
                return Result(213, $"invalid field name <{tableFields.InvalidField}>", null);
            }

            return Database.GetSqlSelect(ConstructSqlSelect(tableFields));
        }

        public static string Post(string tableName, IDictionary<string, string> body)
        {
            if (!IsValid(tableName)) return Result(214, "error, service not supported [POST]", null);

            TableFields tableFields = new TableFields(tableName, TableDict[tableName]);

            if (!tableFields.IsFieldListValid(body))
            {
                return Result(213, $"invalid field name <{tableFields.InvalidField}>", null);
            }

            return Database.SqlTransaction(ConstructSqlInsert(tableFields));
        }


        private static bool IsValid(string tableName)
        {
            return TableDict.ContainsKey(tableName);
        }

        private static string ConstructSqlSelect(TableFields tableFields)
        {
            string sqlText = "";

            sqlText += $"select {tableFields.FieldSelectString} from {tableFields.TableName}";

            if (tableFields.FieldsActive)
            {
                sqlText += " where ";

                int clauseCount = 0;

                foreach (var field in tableFields.AllFields.Where(field => field.FieldActive))
                {
                    if (clauseCount >= 1)
                    {
                        sqlText += " and ";
                    }

                    if (field.FieldType == "date")
                    {
                        sqlText += $"date_format({field.Name},'%Y-%m-%d') = '{field.FieldValue}'";
                    }
                    else
                    {
                        sqlText += $"{field.Name} = {field.FieldValue}";
                    }

                    clauseCount += 1;
                }
            }

            sqlText += $" limit {DataRowReturnLimit}";

            return sqlText;
        }

        private static string ConstructSqlInsert(TableFields tableFields)
        {
            string sqlText = "";

            int fieldCount = 0;

            sqlText += $"insert into {tableFields.TableName} (";

            foreach (var field in tableFields.AllFields.Where(field => field.FieldActive))
            {
                // get the active fields in the correct order 

                if (fieldCount > 0)
                {
                    sqlText += ", ";
                }

                sqlText += field.Name;

                fieldCount += 1;
            }

            sqlText += ") values (";

            int clauseCount = 0;

            foreach (var field in tableFields.AllFields.Where(field => field.FieldActive))
            {
                if (clauseCount >= 1)
                {
                    sqlText += " , ";
                }

                if (field.FieldType == "date")
                {
                    sqlText += $"str_to_date('{field.FieldValue}','%Y-%m-%d')";
                }
                else
                {
                    sqlText += $"{field.FieldValue}";
                }

                clauseCount += 1;
            }

            sqlText += ")";

            return sqlText;
        }

        private class TableFields
        {
            public readonly List<Field> AllFields;
            public readonly string TableName;
            public bool FieldsActive;
            public string InvalidField = "";

            public TableFields(string tableName, string tableDesc)
            {
                TableName = tableName;
                AllFields = new List<Field>();
                FieldsActive = false;

                string[] fieldArray = tableDesc.Split(',');

                int fieldSelectCount = 0;

                foreach (string fieldName in fieldArray)
                {
                    string fieldType = "string";
                    string fieldNameTrim = fieldName.Trim();

                    if (fieldName.IndexOf("(int)", StringComparison.Ordinal) > 0)
                    {
                        fieldNameTrim = fieldName.Replace("(int)", "").Trim();
                        fieldType = "int";
                    }
                    else if (fieldName.IndexOf("(dec)", StringComparison.Ordinal) > 0)
                    {
                        fieldNameTrim = fieldName.Replace("(dec)", "").Trim();
                        fieldType = "decimal";
                    }
                    else if (fieldName.IndexOf("(date)", StringComparison.Ordinal) > 0)
                    {
                        fieldNameTrim = fieldName.Replace("(date)", "").Trim();
                        fieldType = "date";
                    }

                    AllFields.Add(new Field(fieldNameTrim, fieldType));

                    // remove spaces from field list
                    // and set field type

                    if (fieldSelectCount > 0)
                    {
                        FieldSelectString += ", ";
                    }

                    if (fieldType == "date")
                    {
                        FieldSelectString += $"date_format({fieldNameTrim},'%Y-%m-%d') as {fieldNameTrim}";
                    }
                    else
                    {
                        FieldSelectString += $"{fieldNameTrim}";
                    }

                    fieldSelectCount += 1;
                }
            }

            public string FieldSelectString { get; }


            public bool IsFieldListValid(IDictionary<string, string> fieldDict)
            {
                // compare the query string columns to the table definition
                // throw query out if column names do not match


                if (fieldDict == null || fieldDict.Count == 0) return true;

                foreach (var (fieldName, value) in fieldDict)
                {
                    string fieldNameTrim = fieldName.Trim();
                    fieldNameTrim = fieldNameTrim.Replace("\"", "");
                    fieldNameTrim = fieldNameTrim.Replace("'", "");

                    if (IsFieldValid(fieldNameTrim))
                    {
                        // valid column found, so add value and activate query

                        SetFieldValues(fieldNameTrim, value);
                    }
                    else
                    {
                        InvalidField = fieldNameTrim;
                        return false;
                    }
                }

                return true;
            }

            private bool IsFieldValid(string fieldName)
            {
                return AllFields.Any(field => field.Name == fieldName);
            }

            private void SetFieldValues(string fieldName, string value)
            {
                Field fieldFound = AllFields.Find(field => field.Name == fieldName);

                if (fieldFound == null) return;

                fieldFound.FieldActive = true;

                // flag an active query at table level
                FieldsActive = true;

                // remove quoted fields and rely on table definitions

                value = value.Trim();
                value = value.Replace("\"", "");
                value = value.Replace("'", "");

                if (fieldFound.FieldType == "string")
                {
                    fieldFound.FieldValue = "\"" + value + "\"";
                }
                else
                {
                    fieldFound.FieldValue = value;
                }
            }
        }

        private class Field
        {
            public Field(string name, string fieldType)
            {
                Name = name;
                FieldType = fieldType;
                FieldValue = "";
                FieldActive = false;
            }

            public string Name { get; }
            public string FieldType { get; }
            public string FieldValue { get; set; }
            public bool FieldActive { get; set; }
        }
    }
}