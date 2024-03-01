using DapperCodeGenerator.Models;
using static DapperCodeGenerator.Services.TypeMapper;

namespace DapperCodeGenerator.Services
{
    public class FieldNameNormalizer(HttpClient httpClient) : IFieldNameNormalizer
    {
        private string _shortfieldlist = "";
        private string _primaryKey = "";
        private string _modelProperty = "";
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


        public async Task<List<TableField>> NormalizeFieldNames(string rawText, string objectName, string tableName)
        {
            var rawTemplate = await httpClient.GetStringAsync("BlazorDapperTemplate.txt");

            var tableFields = new List<TableField>();
            var fieldNames = rawText.Split("NULL,").Select(x => x.Trim()).ToList();
            var tableHeadings = "";
            var tableRows = "";
            var confirmDeleteRows = "";
            var parametersAddUpdateOnly = string.Empty;
            var counter = 1;
            
            foreach (var fieldName in fieldNames)
            {
                if (fieldName.Length <= 0) continue;
                var tableField = new TableField(fieldName);
                tableFields.Add(tableField);
                
                // Primary key column plus 6 more fields...just a wild guess.
                if (counter is > 1 and < 7)
                {
                    tableHeadings += "<th>" + tableField.ColumnName + "</th>";
                    tableRows += "<td>@" + objectName + "." + tableField.ColumnName + "</td>";
                    _shortfieldlist += tableField.ColumnName + ", ";
                }
                counter++;
            }

            
            counter = 1;
            var sprocOneParam = "";
            var sprocAllParams = "";
            var sprocInsertParams = "";

            for (int i = 0; i < tableFields.Count; i++)
            {
                
            }
            
            foreach (var tableField in tableFields)
            {
                if (tableField.IsRequired && tableField.ColumnName != _primaryKey) _modelProperty += "[Required]\n";
                if (tableField.Length > 0) _modelProperty += "[StringLength(" + tableField.Length + ")]\n";
                _modelProperty += "public " + tableField.DotNetType + " " + tableField.ColumnName + " { get; set; }\n";

                sprocAllParams += "@" + tableField.ColumnName + " " + tableField.DataType + ",\n";

                // The first field is assumed to be the key, so we don't include that on paramters for Insert sproc
                if (counter != 1)
                {
                    sprocInsertParams += "@" + tableField.ColumnName + " " + tableField.DataType + ",\n";
                }

                switch (counter)
                {
                    case 1:
                        sprocOneParam = "@" + tableField.ColumnName + " " + tableField.DataType;
                        _primaryKey = tableField.ColumnName;
                        // The Update stored proc requires the primary key as a parameter, insert doesn't.
                        parametersAddUpdateOnly = "parameters.Add(" + Convert.ToChar(34) + tableField.ColumnName +
                                                  Convert.ToChar(34) + ", " + objectName + "." + tableField.ColumnName +
                                                  ", DbType." + tableField.DbType + ");\n";
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
                        _parametersadd += "parameters.Add(" + Convert.ToChar(34) + tableField.ColumnName +
                                          Convert.ToChar(34) + ", " + objectName + "." + tableField.ColumnName +
                                          ", DbType." + tableField.DbType + ");\n";
                        //inputtag += "<tr><td>" + fld.Fieldname + ":</td><td><input type=" + Convert.ToChar(34) + TypeMapper.GetHtmlType(fld.Codetype) + Convert.ToChar(34) + " @bind=" + Convert.ToChar(34) + objectname + "." + fld.Fieldname + Convert.ToChar(34);
                        var cappedType = GetHtmlType(tableField.DotNetType);
                        cappedType = char.ToUpper(cappedType[0]) + cappedType.Substring(1);
                        //If the data type in SQL is Text, then use TextArea rather than Input'
                        if (tableField.DataType == "text")
                        {
                            cappedType = "TextArea";
                        }

                        if (counter % 2 == 0)
                        {
                            _inputtag += "<div class='row'>\n";
                        }

                        _inputtag += "<div class='col-2'>\n<label for = '" + tableField.ColumnName + "'>" +
                                     tableField.ColumnName + ":</label>\n</div>\n<div class='col-4'>\n<Input" +
                                     cappedType + " @bind-Value = " + Convert.ToChar(34) + objectName + "." +
                                     tableField.ColumnName + Convert.ToChar(34) + " class='form-control'";

                        if (tableField.DataType.Contains("varchar") || tableField.DataType.Contains("text"))
                        {
                            _inputtag += " style='width:100%;'";
                        }

                        //The date ranges don't work in the model, so adding here.
                        if (tableField.DataType.Contains("date"))
                        {
                            _inputtag += " min=" + Convert.ToChar(34) + "1753-01-01" + Convert.ToChar(34) + " max=" +
                                         Convert.ToChar(34) + "9999-12-31" + Convert.ToChar(34);
                        }

                        _inputtag += " id = '" + tableField.ColumnName + "'/></div>\n";
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
                        confirmDeleteRows += "<div class='form-group'>" + tableField.ColumnName + ":@" + objectName +
                                             "." + tableField.ColumnName + "</div>\n";
                        // For the initial List page and date search I show ony the first five fields, you
                        // may need to adjust in your own app.
                        if (counter < 7)
                        {
                            _sqlselectlist += ", " + tableField.ColumnName;
                        }

                        _sqlselectone += ", " + tableField.ColumnName;
                        _sqlfieldsinsert1 += tableField.ColumnName + ", ";
                        _sqlfieldsinsert2 += "@" + tableField.ColumnName + ", ";
                        _sqlupdate += tableField.ColumnName + " = @" + tableField.ColumnName + ", ";
                        //convert dates to string for Like search
                        if (tableField.DataType.Contains("date"))
                        {
                            //Isolate the first date or datetime field
                            _iDateCounter += 1;
                            if (_iDateCounter == 1)
                            {
                                _sqlDateSearchWhereClause = " CAST(" + tableField.ColumnName +
                                                            " AS Date) Between @startDate AND @endDate";
                            }

                            //sqlsearchflds += "CONVERT(varchar(12)," + fld.Fieldname + ",101) AS " + fld.Fieldname + ", ";
                            _sqlsearch += "CONVERT(varchar(12)," + tableField.ColumnName +
                                          ",101) LIKE + '%' + @param + '%' OR ";
                        }

                        //convert numbers to string for like search
                        if (tableField.DataType.Contains("int") || tableField.DataType.Contains("decimal") ||
                            tableField.DataType.Contains("money"))
                        {
                            //sqlsearchflds += "CAST(" + fld.Fieldname + " AS varchar(20)) AS " + fld.Fieldname + ",";
                            _sqlsearch += "CAST(" + tableField.ColumnName +
                                          " AS varchar(20)) LIKE '%' + @param + '%' OR ";
                        }

                        //No convertsion for text fields
                        if (tableField.DataType.Contains("text") || tableField.DataType.Contains("char"))
                        {
                            //sqlsearchflds += fld.Fieldname + ",";
                            _sqlsearch += tableField.ColumnName + " LIKE '%' + @param + '%' OR ";
                        }

                        break;
                    }
                }

                counter += 1;
            }

            sprocAllParams = sprocAllParams[..^2];
            sprocInsertParams = sprocInsertParams[..^2];
            rawTemplate = rawTemplate.Replace("@@PK", _primaryKey);
            rawTemplate = rawTemplate.Replace("@@spOneParam", sprocOneParam);
            rawTemplate = rawTemplate.Replace("@@spAllParams", sprocAllParams);
            rawTemplate = rawTemplate.Replace("@@spInsertParams", sprocInsertParams);
            rawTemplate = rawTemplate.Replace("@@OBJECTNAME", objectName);
            rawTemplate = rawTemplate.Replace("@@MODELCODE", _modelProperty);
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
    }
}