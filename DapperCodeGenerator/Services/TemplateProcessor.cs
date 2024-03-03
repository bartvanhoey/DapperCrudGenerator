using DapperCodeGenerator.Models;
using static System.Convert;
using static System.String;

namespace DapperCodeGenerator.Services
{
    public class TemplateProcessor(ICodeGenerator generator) : ITemplateProcessor
    {
        private string _primaryKey = "", _modelProperties = "", _sqlDelete = "";
        private string _sqlSelectList = "", _sqlSelectOne = "", _sqlInsert = "", _sqlUpdate = "";
        private string _sqlSearch = "", _sqlDateRange = "", _parametersAdd = ""; 
        private string _sqlDateSearchWhereClause = "", _sqlInsertColumnNames = "", _sqlInsertValues = "";
        private int _iDateCounter;

        public async Task Process(CodeTemplate? template)
        {
            var createTableStatement = template?.CreateTableStatement;
            if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return;

            var tableInfo = template?.GetTableInfo();
            var className = template?.GetClassName();
            var confirmDeleteRows = "";
            var parametersAddUpdateOnly = Empty;
            var counter = 1;
            var sprocOneParam = "";
            var sprocAllParams = "";
            var sprocInsertParams = "";

            var tableInfos = tableInfo?.Split("NULL,").Select(x => x.Trim()).ToList();
            if (tableInfos == null) return;

            var columns = tableInfos.Select(info => new Column(info)).ToList();
            foreach (var column in columns)
            {
                if (column.IsRequired && column.Name != _primaryKey) _modelProperties += "[Required]\n";
                if (column.Length > 0) _modelProperties += $"[StringLength({column.Length})]\n";
                _modelProperties += $"public {column.DotNetType} {column.Name} {{ get; set; }}\n";

                // The first field is assumed to be the key, so we don't include that on parameters for Insert sproc
                if (counter > 1) sprocInsertParams += $"@{column.Name} {column.DataType},\n";
                sprocAllParams += $"@{column.Name} {column.DataType},\n";

                switch (counter)
                {
                    case 1:
                        sprocOneParam = $"@{column.Name} {column.DataType}";
                        _primaryKey = column.Name;
                        // The Update stored proc requires the primary key as a parameter, insert doesn't.
                        parametersAddUpdateOnly =
                            $"parameters.Add({ToChar(34)}{column.Name}{ToChar(34)}, {className?.ToLower()}.{column.Name}, DbType.{column.DbType});\n";
                        _sqlSelectOne = $"SELECT {_primaryKey}";
                        _sqlSearch = $"SELECT {columns.GetColumnNames()} FROM {template?.GetTableName()} WHERE ";
                        _sqlDateRange = $"SELECT {columns.GetColumnNames()} FROM {template?.GetTableName()} WHERE ";

                        break;
                    // ===================================================================================================== Input tags for adding and editing, never the primary key.
                    case > 1:
                    {
                        _parametersAdd +=
                            $"parameters.Add({ToChar(34)}{column.Name}{ToChar(34)}, {className?.ToLower()}.{column.Name}, DbType.{column.DbType});\n";
   
                        confirmDeleteRows += $"<div class='form-group'>{column.Name}:@{className?.ToLower()}.{column.Name}</div>\n";
                        // For the initial List page and date search I show ony the first five fields, you
                        // may need to adjust in your own app.
                        if (counter < 7)
                        {
                            _sqlSelectList += ", " + column.Name;
                        }

                        _sqlSelectOne += $", {column.Name}";
                        _sqlInsertColumnNames += $"{column.Name}, ";
                        _sqlInsertValues += $"@{column.Name}, ";
                        _sqlUpdate += $"{column.Name} = @{column.Name}, ";

                        //convert dates to string for Like search
                        if (column.DataType.Contains("date"))
                        {
                            //Isolate the first date or datetime field
                            _iDateCounter += 1;
                            if (_iDateCounter == 1)
                            {
                                _sqlDateSearchWhereClause =
                                    $" CAST({column.Name} AS Date) Between @startDate AND @endDate";
                            }

                            _sqlSearch += $"CONVERT(varchar(12),{column.Name},101) LIKE + '%' + @param + '%' OR ";
                        }

                        //convert numbers to string for like search
                        if (column.DataType.Contains("int") || column.DataType.Contains("decimal") ||
                            column.DataType.Contains("money"))
                        {
                            
                            _sqlSearch += $"CAST({column.Name} AS varchar(20)) LIKE '%' + @param + '%' OR ";
                        }

                        //No conversion for text fields
                        if (column.DataType.Contains("text") || column.DataType.Contains("char"))
                        {
                            _sqlSearch += column.Name + " LIKE '%' + @param + '%' OR ";
                        }

                        break;
                    }
                }

                counter++;
            }

            _sqlDelete = $"DELETE FROM {template?.GetTableName()} WHERE {_primaryKey} = @{_primaryKey}";
            _sqlInsert =
                $"INSERT INTO {template?.GetTableName()}({_sqlInsertColumnNames[..^2]}) VALUES ({_sqlInsertValues[..^2]})";
            _sqlUpdate =
                $"UPDATE {template?.GetTableName()} SET {_sqlUpdate[..^2]} WHERE {_primaryKey} = @{_primaryKey}";
            _sqlSelectList = $"SELECT TOP 30 {_primaryKey} FROM {template?.GetTableName()} ORDER BY {_primaryKey} DESC";
            _sqlSelectOne += $" FROM {template?.GetTableName()} WHERE {_primaryKey}= @{_primaryKey}";

            var replacer = new Replacer
            {
                SprocAllParams = sprocAllParams[..^2],
                SprocInsertParams = sprocInsertParams[..^3],
                PrimaryKey = _primaryKey,
                SprocOneParam = sprocOneParam,
                ObjectName = className?.ToLower(),
                ModelCode = _modelProperties,
                ParametersAddUpdateOnly = parametersAddUpdateOnly,
                ParametersAdd = _parametersAdd,
                InputOneRows = columns.GetInputTextTags(className),
                FieldNamesTableHeading = columns.GetTableHeadings(),
                FieldNamesTableRow = columns.GetTableRows(className),
                ConfirmDeleteRows = confirmDeleteRows,
                SqlInsert = _sqlInsert,
                SqlUpdate = _sqlUpdate,
                SqlSelectList = _sqlSelectList,
                SqlSelectOne = _sqlSelectOne,
                SqlSearch = _sqlSearch[..^4],
                SqlDelete = _sqlDelete,
                SqlDateRange = _sqlDateRange + _sqlDateSearchWhereClause,
                TableName = template?.GetTableName(),
                ClassName = className,
                ServiceName = className + "Service",
                ListPageName = className?.ToLower() + "list",
                ListName = template?.GetListName(),
                FieldList = template?.CreateTableStatement,
                NameSpaceName = template?.NamespaceName
            };


            var generateResult = await generator.Generate(replacer);
        }


        
    }
}