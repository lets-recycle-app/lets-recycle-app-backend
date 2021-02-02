using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiCore
{
    public class Table
    {
        public readonly List<Field> AllFields;
        public readonly string TableName;
        public string InvalidField = "";
        public bool QueryActive;

        public Table(string tableName, string fieldTextString)
        {
            TableName = tableName;
            AllFields = new List<Field>();
            QueryActive = false;

            string[] fieldArray = fieldTextString.Split(',');

            int fieldCount = 0;

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

                if (fieldCount > 0)
                {
                    FieldTextString += ", ";
                }

                if (fieldType == "date")
                {
                    FieldTextString += $"date_format({fieldNameTrim},'%Y-%m-%d') as {fieldNameTrim}";
                }
                else
                {
                    FieldTextString += $"{fieldNameTrim}";
                }

                fieldCount += 1;
            }
        }

        public string FieldTextString { get; }


        private bool IsField(string fieldName)
        {
            return AllFields.Any(field => field.Name == fieldName);
        }

        public bool IsQueryValid(IReadOnlyCollection<(string, string)> query)
        {
            // compare the query string columns to the table definition
            // throw query out if column names do not match

            if (query == null || query.Count == 0) return true;

            foreach (var (fieldName, value) in query)
            {
                if (IsField(fieldName))
                {
                    // valid column found, so add value and activate query
                    SetFieldQuery(fieldName, value);
                }
                else
                {
                    InvalidField = fieldName;
                    return false;
                }
            }

            return true;
        }


        private void SetFieldQuery(string fieldName, string value)
        {
            Field fieldFound = AllFields.Find(field => field.Name == fieldName);

            if (fieldFound == null) return;

            fieldFound.QueryActive = true;

            // flag an active query at table level
            QueryActive = true;

            // remove quoted fields and rely on table definitions

            value = value.Replace("\"", "");
            value = value.Replace("'", "");

            if (fieldFound.FieldType == "string")
            {
                fieldFound.QueryValue = "\"" + value + "\"";
            }
            else
            {
                fieldFound.QueryValue = value;
            }
        }
    }

    public class Field
    {
        public Field(string name, string fieldType)
        {
            Name = name;
            FieldType = fieldType;
            QueryValue = "";
            QueryActive = false;
        }

        public string Name { get; }
        public string FieldType { get; }
        public string QueryValue { get; set; }
        public bool QueryActive { get; set; }
    }
}