using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiCore
{
    public class Table
    {
        public readonly List<Field> AllFields;
        public readonly string TableName;
        public bool FieldsActive;
        public string InvalidField = "";

        public Table(string tableName, string fieldTextString)
        {
            TableName = tableName;
            AllFields = new List<Field>();
            FieldsActive = false;

            string[] fieldArray = fieldTextString.Split(',');

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

    public class Field
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