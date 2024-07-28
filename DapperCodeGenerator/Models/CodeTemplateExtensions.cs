using Pluralize.NET.Core;
using static System.Convert;

namespace DapperCodeGenerator.Models;

public static class CodeTemplateExtensions
{
    public static string? GetTableName(this Template template)
    {
        var createTableStatement = template.CreateTableStatement;
        if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return null;
        var tableInfo = createTableStatement.Replace("CREATE TABLE [dbo].[", "");
        return tableInfo[..tableInfo.IndexOf("]", StringComparison.Ordinal)].Replace("\n", "");
    }

    public static string? GetTableInfo(this Template template)
    {
        var createTableStatement = template.CreateTableStatement;
        if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return null;
        var tableInfo = createTableStatement.Replace("CREATE TABLE [dbo].[", "");
        var lastIndexClosingBracket = tableInfo.LastIndexOf(")", StringComparison.Ordinal);
        if (lastIndexClosingBracket > 0) tableInfo = tableInfo[..(lastIndexClosingBracket + 1)];
        var lastIndexConstraint = tableInfo.IndexOf("CONSTRAINT", StringComparison.Ordinal);
        if (lastIndexConstraint > 0) tableInfo = tableInfo[..lastIndexConstraint];
        tableInfo = tableInfo.Trim();
        var firstParentheses = tableInfo.IndexOf("(", StringComparison.Ordinal);
        tableInfo = tableInfo[(firstParentheses + 1)..];
        tableInfo = tableInfo.Replace("\n", "").Replace("\t", "");
        return tableInfo;
    }
        
    public static string? GetClassName(this Template template)
    {
        var tableName = template.GetTableName();
        if (tableName == null) return null;
        var charArray = tableName.ToCharArray();
        charArray[0] = char.ToUpper(charArray[0]);
        var className = new string(charArray);
        if (className[..3] == "Tbl") className = className[3..];
        return className;
    }


    public static string? GetListName(this Template template)
    {
        var className = template.GetClassName();
        if (className == null) return null;
        var objectName = className.ToLower();
        var listName = new Pluralizer().Pluralize(objectName);
        if (listName == objectName) listName += "s";
        return listName;
    }

    public static string GetSqlSelectOneFromClause(this Template? template, string primaryKey) 
        => $" FROM {template?.GetTableName()} WHERE {primaryKey}= @{primaryKey}";

    public static string GetSqlSelectListStatement(this Template? template, string primaryKey) 
        => $"SELECT TOP 30 {primaryKey} FROM {template?.GetTableName()} ORDER BY {primaryKey} DESC";

    public static string GetSqlDeleteStatement(this Template? template, string primaryKey) 
        => $"DELETE FROM {template?.GetTableName()} WHERE {primaryKey} = @{primaryKey}";

    public static string GetSqlUpdateStatement(this Template? template, string primaryKey, string sqlUpdate) 
        => $"UPDATE {template?.GetTableName()} SET {sqlUpdate[..^2]} WHERE {primaryKey} = @{primaryKey}";

    public static string GetSqlInsertStatement(this Template? template, string sqlInsertColumnNames, string sqlInsertValues)
        => $"INSERT INTO {template?.GetTableName()}({sqlInsertColumnNames[..^2]}) VALUES ({sqlInsertValues[..^2]})";
    
    public static string GetSqlSearchBaseStatement(this Template? template, IEnumerable<Column> columns) 
        => $"SELECT {columns.GetColumnNames()} FROM {template?.GetTableName()} WHERE ";
    
    public static string GetSqlDateRangeBaseStatement(this Template? template, IEnumerable<Column> columns) 
        => $"SELECT {columns.GetColumnNames()} FROM {template?.GetTableName()} WHERE ";
    
    public static string GetParametersAddUpdateOnly(this Template? template, Column column) 
        => $"parameters.Add({ToChar(34)}{column.Name}{ToChar(34)}, {template?.GetClassName()?.ToLower()}.{column.Name}, DbType.{column.DbType});\n";
    public static string GetParametersAdd(this Template? template, Column column) 
        => $"parameters.Add({ToChar(34)}{column.Name}{ToChar(34)}, {template?.GetClassName()?.ToLower()}.{column.Name}, DbType.{column.DbType});\n";
    
    public static string GetConfirmDeleteRows(this Template? template, Column column) 
        => $"<div class='form-group'>{column.Name}:@{template?.GetClassName()?.ToLower()}.{column.Name}</div>\n";
}