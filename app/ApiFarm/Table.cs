using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiFarm
{
    public class Table
    {
        public readonly List<Field> AllFields;
        public readonly string TableName;

        public Table(string tableName, string fieldTextString)
        {
            TableName = tableName;
            AllFields = new List<Field>();

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


        public bool IsField(string fieldName)
        {
            return AllFields.Any(field => field.Name == fieldName);
        }

        public bool SetFieldQuery(string fieldName, string value)
        {
            Field fieldFound = AllFields.Find(field => field.Name == fieldName);

            if (fieldFound == null) return false;

            fieldFound.QueryActive = true;

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

            return true;
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