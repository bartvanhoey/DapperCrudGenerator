using DapperCodeGenerator.Models;
using static DapperCodeGenerator.Services.TypeMapper;

namespace DapperCodeGenerator.Services
{
    public class TableInfoService(ICodeGenerator generator) : ITableInfoService
    {
        private string _columnNames = "", _primaryKey = "", _modelProperty = "", _sqlDelete = "";
        private string _sqlSelectList = "", _sqlSelectOne = "", _sqlInsert = "", _sqlUpdate = "";
        private string _sqlSearch = "", _sqlDateRange = "", _parametersAdd = "", _inputTag = "";
        private string _sqlDateSearchWhereClause = "", _sqlFieldsInsert1 = "", _sqlFieldsInsert2 = "";
        private int _iDateCounter;

        public async Task Process(CodeTemplate? codeTemplate)
        {
            var createTableStatement = codeTemplate?.CreateTableStatement;
            if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return;
            
            var tableInfo = codeTemplate?.GetTableInfo();
            var className = codeTemplate?.GetClassName();
            var tableFields = new List<TableColumnInfo>();
            var tableHeadings = "";
            var tableRows = "";
            var confirmDeleteRows = "";
            var parametersAddUpdateOnly = string.Empty;
            var counter = 1;

            var fieldNames = tableInfo?.Split("NULL,").Select(x => x.Trim()).ToList();
            if (fieldNames == null) return;
            foreach (var fieldName in fieldNames)
            {
                if (fieldName.Length <= 0) continue;
                var tableField = new TableColumnInfo(fieldName);
                tableFields.Add(tableField);

                // Primary key column plus 6 more fields...just a wild guess.
                if (counter is > 1 and < 7)
                {
                    tableHeadings += "<th>" + tableField.ColumnName + "</th>";
                    tableRows += "<td>@" + className?.ToLower() + "." + tableField.ColumnName + "</td>";
                    _columnNames += tableField.ColumnName + ", ";
                }

                counter++;
            }


            counter = 1;
            var sprocOneParam = "";
            var sprocAllParams = "";
            var sprocInsertParams = "";

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
                                                  Convert.ToChar(34) + ", " + className?.ToLower() + "." +
                                                  tableField.ColumnName +
                                                  ", DbType." + tableField.DbType + ");\n";
                        _sqlDelete = $"DELETE FROM {codeTemplate.GetTableName()} WHERE {_primaryKey} = @{_primaryKey}";
                        _sqlSelectList = "SELECT TOP 30 " + _primaryKey;
                        _sqlSelectOne = "SELECT " + _primaryKey;
                        _sqlInsert = "INSERT INTO " + codeTemplate.GetTableName() + "(";
                        _sqlUpdate = "UPDATE " + codeTemplate.GetTableName() + " SET ";
                        //Remove comma and space from end of shortfieldlist.
                        _sqlSearch = "SELECT " + _columnNames[..^2] + " FROM " + codeTemplate.GetTableName() +
                                     " WHERE ";
                        _sqlDateRange = "SELECT " + _columnNames[..^2] + " FROM " + codeTemplate.GetTableName() +
                                        " WHERE ";
                        break;
                    // ===================================================================================================== Input tags for adding and editing, never the primary key.
                    case > 1:
                    {
                        _parametersAdd += "parameters.Add(" + Convert.ToChar(34) + tableField.ColumnName +
                                          Convert.ToChar(34) + ", " + className?.ToLower() + "." +
                                          tableField.ColumnName +
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
                            _inputTag += "<div class='row'>\n";
                        }

                        _inputTag += "<div class='col-2'>\n<label for = '" + tableField.ColumnName + "'>" +
                                     tableField.ColumnName + ":</label>\n</div>\n<div class='col-4'>\n<Input" +
                                     cappedType + " @bind-Value = " + Convert.ToChar(34) + className?.ToLower() + "." +
                                     tableField.ColumnName + Convert.ToChar(34) + " class='form-control'";

                        if (tableField.DataType.Contains("varchar") || tableField.DataType.Contains("text"))
                        {
                            _inputTag += " style='width:100%;'";
                        }

                        //The date ranges don't work in the model, so adding here.
                        if (tableField.DataType.Contains("date"))
                        {
                            _inputTag += " min=" + Convert.ToChar(34) + "1753-01-01" + Convert.ToChar(34) + " max=" +
                                         Convert.ToChar(34) + "9999-12-31" + Convert.ToChar(34);
                        }

                        _inputTag += " id = '" + tableField.ColumnName + "'/></div>\n";
                        if (counter % 2 != 0)
                        {
                            _inputTag += "</div>\n";
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
                        confirmDeleteRows += "<div class='form-group'>" + tableField.ColumnName + ":@" +
                                             className?.ToLower() +
                                             "." + tableField.ColumnName + "</div>\n";
                        // For the initial List page and date search I show ony the first five fields, you
                        // may need to adjust in your own app.
                        if (counter < 7)
                        {
                            _sqlSelectList += ", " + tableField.ColumnName;
                        }

                        _sqlSelectOne += ", " + tableField.ColumnName;
                        _sqlFieldsInsert1 += tableField.ColumnName + ", ";
                        _sqlFieldsInsert2 += "@" + tableField.ColumnName + ", ";
                        _sqlUpdate += tableField.ColumnName + " = @" + tableField.ColumnName + ", ";
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
                            _sqlSearch += "CONVERT(varchar(12)," + tableField.ColumnName +
                                          ",101) LIKE + '%' + @param + '%' OR ";
                        }

                        //convert numbers to string for like search
                        if (tableField.DataType.Contains("int") || tableField.DataType.Contains("decimal") ||
                            tableField.DataType.Contains("money"))
                        {
                            //sqlsearchflds += "CAST(" + fld.Fieldname + " AS varchar(20)) AS " + fld.Fieldname + ",";
                            _sqlSearch += "CAST(" + tableField.ColumnName +
                                          " AS varchar(20)) LIKE '%' + @param + '%' OR ";
                        }

                        //No convertsion for text fields
                        if (tableField.DataType.Contains("text") || tableField.DataType.Contains("char"))
                        {
                            //sqlsearchflds += fld.Fieldname + ",";
                            _sqlSearch += tableField.ColumnName + " LIKE '%' + @param + '%' OR ";
                        }

                        break;
                    }
                }

                counter += 1;
            }

            //Remove extra comma and space at the end of field names.
            _sqlFieldsInsert1 = _sqlFieldsInsert1[..^2];
            _sqlFieldsInsert2 = _sqlFieldsInsert2[..^2];
            _sqlUpdate = _sqlUpdate[..^2];
            _sqlInsert += _sqlFieldsInsert1 + ") VALUES (" + _sqlFieldsInsert2 + ")";
            _sqlUpdate += " WHERE " + _primaryKey + " = @" + _primaryKey;
            _sqlSelectList += " FROM " + codeTemplate?.GetTableName() + " ORDER BY " + _primaryKey + " DESC";
            _sqlSelectOne += " FROM " + codeTemplate?.GetTableName() + " WHERE " + _primaryKey + "= @" + _primaryKey;
            _sqlInsert += _sqlFieldsInsert1[..^2] + ") VALUES (" + _sqlFieldsInsert2[..^2] + ")";
            _sqlUpdate += _sqlUpdate[..^2];
            _sqlUpdate += " WHERE " + _primaryKey + " = @" + _primaryKey;
            _sqlSelectList += " FROM " + codeTemplate?.GetTableName() + " ORDER BY " + _primaryKey + " DESC";
            _sqlSelectOne += " FROM " + codeTemplate?.GetTableName() + " WHERE " + _primaryKey + "= @" + _primaryKey;

            var replacer = new Replacer
            {
                SprocAllParams = sprocAllParams[..^2],
                SprocInsertParams = sprocInsertParams[..^2],
                PrimaryKey = _primaryKey,
                SprocOneParam = sprocOneParam,
                ObjectName = className?.ToLower(),
                ModelCode = _modelProperty,
                ParametersAddUpdateOnly = parametersAddUpdateOnly,
                ParametersAdd = _parametersAdd,
                InputOneRows = _inputTag,
                FieldNamesTableHeading = tableHeadings,
                FieldNamesTableRow = tableRows,
                ConfirmDeleteRows = confirmDeleteRows,
                SqlInsert = _sqlInsert,
                SqlUpdate = _sqlUpdate,
                SqlSelectList = _sqlSelectList,
                SqlSelectOne = _sqlSelectOne,
                SqlSearch = _sqlSearch[..^4],
                SqlDelete = _sqlDelete,
                SqlDateRange = _sqlDateRange + _sqlDateSearchWhereClause,
                TableName = codeTemplate?.GetTableName(),
                ClassName = className,
                ServiceName = className + "Service",
                ListPageName = className?.ToLower() + "list",
                ListName = codeTemplate?.GetListName(),
                FieldList = codeTemplate?.CreateTableStatement,
                NameSpaceName = codeTemplate?.NamespaceName
            };


            generator.SetReplacer(replacer);
            
            var sqlConnectionConfiguration = await generator.SqlConnectionConfiguration();
            var insertStoredProcedure = await generator.InsertStoredProcedure();
            var updateStoredProcedure = await generator.UpdateStoredProcedure();
            var selectStoredProcedure = await generator.SelectStoredProcedure();
            var selectOneStoredProcedure = await generator.SelectOneStoredProcedure();
            var selectLikeStoredProcedure = await generator.SelectLikeStoredProcedure();
            var selectDataRangeStoredProcedure = await generator.SelectDataRangeStoredProcedure();
            var deleteStoredProcedure = await generator.DeleteStoredProcedure();
            var modelClass = await generator.ModelClass();
            var serviceInterface = await generator.CrudServiceInterface();
            var serviceClass = await generator.CrudServiceClass();
            var addEditRazorPage = await generator.AddEditRazorPage();
            var listRazorPage = await generator.ListRazorPage();
            var reportRazorPage = await generator.ReportRazorPage();
            var deleteRazorPage = await generator.DeleteRazorPage();
            var navLinkToList = await generator.NavLinkToList();
            var cssStyling = await generator.CssStyling();
        }
    }
}