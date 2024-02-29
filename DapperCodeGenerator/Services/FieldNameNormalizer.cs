using System.Text.RegularExpressions;
using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services
{
    public class FieldNameNormalizer(HttpClient httpClient) : IFieldNameNormalizer
    {
        private string _shortfieldlist = "";
        private string _primaryKey = "";
        private string _modelfield = "";
        private string _sqldelete = "";
        private string _sqlselectlist = "";
        private string _sqlselectone = "";
        private string _sqlinsert = "";
        private string _sqlupdate = "";
        private string _sqlsearch = "";
        private string _sqldaterange = "";
        private string _parametersadd = "";
        private string _inputtag = "";
        private string _sqlfieldsinsert1 = "";
        private string _sqlfieldsinsert2 = "";
        private int _iDateCounter;
        private string _sqlDateSearchWhereClause = "";


        public async Task<List<Tablefield>> NormalizeFieldNames(string rawText, string objectName, string tableName)
        {
            var rawTemplate = await httpClient.GetStringAsync("BlazorDapperTemplate.txt");

            var tableFields = new List<Tablefield>();
            var fieldNames = rawText.Split("NULL,").Select(x => x.Trim()).ToList();
            var tableHeadings = "";
            var tableRows = "";
            var confirmDeleteRows = "";
            var parametersAddUpdateOnly = string.Empty;
            var counter = 1;
            foreach (var fieldNameTrimmed in fieldNames)
            {
                if (fieldNameTrimmed.Length <= 0) continue;
                var columnName = fieldNameTrimmed.Substring(1, fieldNameTrimmed.IndexOf(']') - 1);
                columnName = char.ToUpper(columnName[0]) + columnName[1..];
                // Primary key column plus 6 more fields...just a wild guess.
                if (counter is > 1 and < 7)
                {
                    tableHeadings += "<th>" + columnName + "</th>";
                    tableRows += "<td>@" + objectName + "." + columnName + "</td>";
                    _shortfieldlist += columnName + ", ";
                }

                var isRequired = fieldNameTrimmed[^4..] == " NOT";
                // Isolate the data time
                var datatype = fieldNameTrimmed[(fieldNameTrimmed.IndexOf(']') + 2)..];
                datatype = datatype.Replace("IDENTITY(1,1) ", "");
                datatype = datatype.Replace("IDENTITY (1, 1) ", "");
                datatype = datatype.Replace("NULL", "").Replace("NOT", "");
                datatype = datatype.Replace("[", "").Replace("]", "").Trim();

                //Remove the parens and number (if any) from the datatype.
                var temptype = RemoveNonAlphaCharacters(datatype).Trim().ToLower();
                var nettype = TypeMapper.GetNetType(temptype);
                var dbtype = TypeMapper.GetDBType(temptype);
                // If it's any kind of "char" field, grab the length
                var maxlength = 0;
                if (datatype.Contains("char"))
                {
                    // Look for the length if it's any kind of char field.
                    var charlength = KeepNumbersOnly(datatype);
                    int number;
                    var success = int.TryParse(charlength, out number);
                    if (success)
                    {
                        maxlength = number;
                    }
                }

                var tableField = new Tablefield(columnName, datatype, nettype, dbtype, maxlength, isRequired);
                tableFields.Add(tableField);
                counter += 1;
            }

            // Looping through here, throws error in generate() method.
            counter = 1;

            var sprocOneparam = "";
            var sprocAllparams = "";
            var sprocInsertparams = "";


            foreach (var tableField in tableFields)
            {
                if (tableField.IsRequired && tableField.Fieldname != _primaryKey)
                {
                    _modelfield += "[Required]\n";
                }

                if (tableField.Length > 0)
                {
                    _modelfield += "[StringLength(" + tableField.Length + ")]\n";
                }

                _modelfield += "public " + tableField.Codetype + " " + tableField.Fieldname + " { get; set; }\n";
                sprocAllparams += "@" + tableField.Fieldname + " " + tableField.SQLtype + ",\n";
                // The first field is assumed to be the key, so we don't include that on paramters for Insert sproc
                if (counter != 1)
                {
                    sprocInsertparams += "@" + tableField.Fieldname + " " + tableField.SQLtype + ",\n";
                }

                switch (counter)
                {
                    case 1:
                        sprocOneparam = "@" + tableField.Fieldname + " " + tableField.SQLtype;
                        _primaryKey = tableField.Fieldname;
                        // The Update stored proc requires the primary key as a parameter, insert doesn't.
                        parametersAddUpdateOnly = "parameters.Add(" + Convert.ToChar(34) + tableField.Fieldname +
                                                  Convert.ToChar(34) + ", " + objectName + "." + tableField.Fieldname +
                                                  ", DbType." + tableField.DBtype + ");\n";
                        _sqldelete = $"DELETE FROM {tableName} WHERE {_primaryKey} = @{_primaryKey}";
                        _sqlselectlist = "SELECT TOP 30 " + _primaryKey;
                        _sqlselectone = "SELECT " + _primaryKey;
                        _sqlinsert = "INSERT INTO " + tableName + "(";
                        _sqlupdate = "UPDATE " + tableName + " SET ";
                        //Remove comma and space from end of shortfieldlist.
                        _sqlsearch = "SELECT " + _shortfieldlist[..^2] + " FROM " + tableName + " WHERE ";
                        _sqldaterange = "SELECT " + _shortfieldlist[..^2] + " FROM " + tableName + " WHERE ";
                        break;
                    // ===================================================================================================== Input tags for adding and editing, never the primary key.
                    case > 1:
                    {
                        _parametersadd += "parameters.Add(" + Convert.ToChar(34) + tableField.Fieldname +
                                         Convert.ToChar(34) + ", " + objectName + "." + tableField.Fieldname +
                                         ", DbType." + tableField.DBtype + ");\n";
                        //inputtag += "<tr><td>" + fld.Fieldname + ":</td><td><input type=" + Convert.ToChar(34) + TypeMapper.GetHtmlType(fld.Codetype) + Convert.ToChar(34) + " @bind=" + Convert.ToChar(34) + objectname + "." + fld.Fieldname + Convert.ToChar(34);
                        var cappedType = TypeMapper.GetHtmlType(tableField.Codetype);
                        cappedType = char.ToUpper(cappedType[0]) + cappedType.Substring(1);
                        //If the data type in SQL is Text, then use TextArea rather than Input'
                        if (tableField.SQLtype == "text")
                        {
                            cappedType = "TextArea";
                        }

                        if (counter % 2 == 0)
                        {
                            _inputtag += "<div class='row'>\n";
                        }

                        _inputtag += "<div class='col-2'>\n<label for = '" + tableField.Fieldname + "'>" +
                                    tableField.Fieldname + ":</label>\n</div>\n<div class='col-4'>\n<Input" +
                                    cappedType + " @bind-Value = " + Convert.ToChar(34) + objectName + "." +
                                    tableField.Fieldname + Convert.ToChar(34) + " class='form-control'";

                        if (tableField.SQLtype.Contains("varchar") || tableField.SQLtype.Contains("text"))
                        {
                            _inputtag += " style='width:100%;'";
                        }

                        //The date ranges don't work in the model, so adding here.
                        if (tableField.SQLtype.Contains("date"))
                        {
                            _inputtag += " min=" + Convert.ToChar(34) + "1753-01-01" + Convert.ToChar(34) + " max=" +
                                        Convert.ToChar(34) + "9999-12-31" + Convert.ToChar(34);
                        }

                        _inputtag += " id = '" + tableField.Fieldname + "'/></div>\n";
                        if (counter % 2 != 0)
                        {
                            _inputtag += "</div>\n";
                        }

                        //if (TypeMapper.GetHtmlType(fld.Codetype) == "date")
                        //{
                        //    inputtag += " min=" + Convert.ToChar(34) + "1753-01-01" + Convert.ToChar(34) + " max=" + Convert.ToChar(34) + "9999-12-31" + Convert.ToChar(34);
                        //}
                        //if (fld.Length > 0)
                        //{
                        //    inputtag += " maxlength=" + fld.Length.ToString();
                        //}
                        //if (fld.Codetype == "decimal")
                        //{
                        //    inputtag += " step=" + Convert.ToChar(34) + "any" + Convert.ToChar(34);
                        //}
                        //if (fld.IsRequired)
                        //{
                        //    inputtag += " required ";
                        //}
                        //inputtag += "></td></tr>";
                        confirmDeleteRows += "<div class='form-group'>" + tableField.Fieldname + ":@" + objectName +
                                             "." + tableField.Fieldname + "</div>\n";
                        // For the initial List page and date search I show ony the first five fields, you
                        // may need to adjust in your own app.
                        if (counter < 7)
                        {
                            _sqlselectlist += ", " + tableField.Fieldname;
                        }

                        _sqlselectone += ", " + tableField.Fieldname;
                        _sqlfieldsinsert1 += tableField.Fieldname + ", ";
                        _sqlfieldsinsert2 += "@" + tableField.Fieldname + ", ";
                        _sqlupdate += tableField.Fieldname + " = @" + tableField.Fieldname + ", ";
                        //convert dates to string for Like search
                        if (tableField.SQLtype.Contains("date"))
                        {
                            //Isolate the first date or datetime field
                            _iDateCounter += 1;
                            if (_iDateCounter == 1)
                            {
                                _sqlDateSearchWhereClause = " CAST(" + tableField.Fieldname +
                                                           " AS Date) Between @startDate AND @endDate";
                            }

                            //sqlsearchflds += "CONVERT(varchar(12)," + fld.Fieldname + ",101) AS " + fld.Fieldname + ", ";
                            _sqlsearch += "CONVERT(varchar(12)," + tableField.Fieldname +
                                         ",101) LIKE + '%' + @param + '%' OR ";
                        }

                        //convert numbers to string for like search
                        if (tableField.SQLtype.Contains("int") || tableField.SQLtype.Contains("decimal") ||
                            tableField.SQLtype.Contains("money"))
                        {
                            //sqlsearchflds += "CAST(" + fld.Fieldname + " AS varchar(20)) AS " + fld.Fieldname + ",";
                            _sqlsearch += "CAST(" + tableField.Fieldname +
                                         " AS varchar(20)) LIKE '%' + @param + '%' OR ";
                        }

                        //No convertsion for text fields
                        if (tableField.SQLtype.Contains("text") || tableField.SQLtype.Contains("char"))
                        {
                            //sqlsearchflds += fld.Fieldname + ",";
                            _sqlsearch += tableField.Fieldname + " LIKE '%' + @param + '%' OR ";
                        }

                        break;
                    }
                }

                counter += 1;
            }

            sprocAllparams = sprocAllparams[..^2];
            sprocInsertparams = sprocInsertparams[..^2];
            rawTemplate = rawTemplate.Replace("@@PK", _primaryKey);
            rawTemplate = rawTemplate.Replace("@@spOneParam", sprocOneparam);
            rawTemplate = rawTemplate.Replace("@@spAllParams", sprocAllparams);
            rawTemplate = rawTemplate.Replace("@@spInsertParams", sprocInsertparams);
            rawTemplate = rawTemplate.Replace("@@OBJECTNAME", objectName);
            rawTemplate = rawTemplate.Replace("@@MODELCODE", _modelfield);
            rawTemplate = rawTemplate.Replace("@@PARAMETERADDUPDATEONLY", parametersAddUpdateOnly);
            rawTemplate = rawTemplate.Replace("@@PARAMETERSADD", _parametersadd);
            rawTemplate = rawTemplate.Replace("@@PRIMARYKEY", _primaryKey);
            rawTemplate = rawTemplate.Replace("@@INPUTONEROWS", _inputtag);
            rawTemplate = rawTemplate.Replace("@@FIELDNAMESTABLEHEADING", tableHeadings);
            rawTemplate = rawTemplate.Replace("@@FIELDNAMESTABLEROW", tableRows);
            rawTemplate = rawTemplate.Replace("@@CONFIRMDELETEROWS", confirmDeleteRows);
            //Remove extra comma and space at the end of field names.
            _sqlfieldsinsert1 = _sqlfieldsinsert1[..^2];
            _sqlfieldsinsert2 = _sqlfieldsinsert2[..^2];
            _sqlupdate = _sqlupdate[..^2];
            _sqlinsert += _sqlfieldsinsert1 + ") VALUES (" + _sqlfieldsinsert2 + ")";
            _sqlupdate += " WHERE " + _primaryKey + " = @" + _primaryKey;
            _sqlselectlist += " FROM " + tableName + " ORDER BY " + _primaryKey + " DESC";
            _sqlselectone += " FROM " + tableName + " WHERE " + _primaryKey + "= @" + _primaryKey;

            rawTemplate = rawTemplate.Replace("@@SQLINSERT", _sqlinsert);
            rawTemplate = rawTemplate.Replace("@@SQLUPDATE", _sqlupdate);
            rawTemplate = rawTemplate.Replace("@@SQLSEARCH", _sqlsearch[..^4]);
            rawTemplate = rawTemplate.Replace("@@SQLSELECTLIST", _sqlselectlist);
            rawTemplate = rawTemplate.Replace("@@SQLSELECTONE", _sqlselectone);
            rawTemplate = rawTemplate.Replace("@@SQLDELETE", _sqldelete);
            rawTemplate = rawTemplate.Replace("@@SQLDATERANGE", _sqldaterange + _sqlDateSearchWhereClause);
            rawTemplate = rawTemplate.Replace("@@TABLENAME", tableName);

            return tableFields;
        }

        private static string RemoveNonAlphaCharacters(string input) =>
            Regex.Replace(input, @"[^a-zA-Z\._]", string.Empty);

        private static string KeepNumbersOnly(string input) => Regex.Replace(input, @"[^0-9.]", string.Empty);
    }
}