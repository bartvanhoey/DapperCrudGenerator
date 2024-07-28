using DapperCodeGenerator.Models;
using static System.String;

namespace DapperCodeGenerator.Services
{
    public class TemplateProcessor(ICodeGenerator generator) : ITemplateProcessor
    {
        private string _primaryKey = "", _modelProperties = "", _sqlDelete = "", _sqlSelectList = "", _sqlSelectOne = "", _sqlInsert = "", _sqlUpdate = "";
        private string _sqlSearch = "", _sqlDateRange = "", _parametersAdd = "", _sqlDateSearchWhereClause = "", _sqlInsertColumnNames = "", _sqlInsertValues = "";
        private int _dateCounter;

        public async Task Process(Template? template)
        {
            var createTableStatement = template?.CreateTableStatement;
            if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return;

            var confirmDeleteRows = "";
            var parametersAddUpdateOnly = Empty;
            var sprocOneParam = "";
            var sprocAllParams = "";
            var sprocInsertParams = "";
            
            var tableInfos = template?.GetTableInfo()?.Split("NULL,").Select(x => x.Trim()).ToList();
            if (tableInfos == null) throw new ArgumentNullException();

            var columns = tableInfos.Select(info => new Column(info)).ToList();
            for (var index = 0; index < columns.Count; index++)
            {
                var column = columns[index];
                _modelProperties += column.GetModelProperties(_primaryKey);
                sprocAllParams += $"@{column.Name} {column.DataType},\n"; //first column is assumed Primary Key
                if (index == 0)
                {
                    _primaryKey = column.Name;
                    _sqlSelectOne = $"SELECT {_primaryKey}";
                    _sqlSearch = template.GetSqlSearchBaseStatement(columns);
                    _sqlDateRange = template.GetSqlDateRangeBaseStatement( columns);
                    sprocOneParam = $"@{column.Name} {column.DataType}";
                    parametersAddUpdateOnly = template.GetParametersAddUpdateOnly(column);
                }
                else
                {
                    _parametersAdd += template.GetParametersAdd(column);
                    _sqlSelectOne += $", {column.Name}";
                    _sqlInsertColumnNames += $"{column.Name}, ";
                    _sqlInsertValues += $"@{column.Name}, ";
                    _sqlUpdate += $"{column.Name} = @{column.Name}, ";
                    _sqlSearch += column.GetSqlSearchStatement();
                    _sqlDateSearchWhereClause = column.GetSqlDateSearchWhereClause(_dateCounter);
                    if (index <= 7) _sqlSelectList += $", {column.Name}"; // show only first 7 columns
                    if (!IsNullOrWhiteSpace(_sqlDateSearchWhereClause)) _dateCounter++;
                    sprocInsertParams += $"@{column.Name} {column.DataType},\n";
                    confirmDeleteRows += template.GetConfirmDeleteRows(column);
                }
            }
            _sqlDelete = template.GetSqlDeleteStatement(_primaryKey);
            _sqlInsert = template.GetSqlInsertStatement(_sqlInsertColumnNames, _sqlInsertValues);
            _sqlUpdate = template.GetSqlUpdateStatement(_primaryKey, _sqlUpdate);
            _sqlSelectList =template.GetSqlSelectListStatement( _primaryKey);
            _sqlSelectOne += template.GetSqlSelectOneFromClause(_primaryKey);

            var replacer = new Replacer(template, columns)
            {
                SprocAllParams = sprocAllParams[..^2],
                SprocInsertParams = sprocInsertParams[..^3],
                PrimaryKey = _primaryKey,
                SprocOneParam = sprocOneParam,
                ModelCode = _modelProperties,
                ParametersAddUpdateOnly = parametersAddUpdateOnly,
                ParametersAdd = _parametersAdd,
                ConfirmDeleteRows = confirmDeleteRows,
                SqlInsert = _sqlInsert,
                SqlUpdate = _sqlUpdate,
                SqlSelectList = _sqlSelectList,
                SqlSelectOne = _sqlSelectOne,
                SqlSearch = _sqlSearch[..^4],
                SqlDelete = _sqlDelete,
                SqlDateRange = _sqlDateRange + _sqlDateSearchWhereClause,
            };

            var generateResult = await generator.Generate(replacer);
        }

        
    }
}