using static System.Convert;
using static System.String;
using static DapperCodeGenerator.Services.TypeMapper;

namespace DapperCodeGenerator.Models;

public static class ColumnExtensions
{
    public static string GetColumnNames(this IEnumerable<Column> columns)
        => columns.Where((_, index) => index > 0).Aggregate(Empty, (current, t) => current + t.Name + ", ")[..^2];

    public static string GetTableRows(this IEnumerable<Column> columns, string? className)
        => columns.Where((_, index) => index > 0).Aggregate(Empty, (current, column) =>
            $"{current}<td>@{className?.ToLower()}.{column.Name}</td>");

    public static string GetTableHeadings(this IEnumerable<Column> columns)
        => columns.Where((_, index) => index > 0)
            .Aggregate(Empty, (current, column) => current + $"<th>{column.Name}</th>");

    public static string GetInputTextTags(this IEnumerable<Column> columns, string? className)
    {
        var inputTag = "";
        foreach (var column in columns)
        {
            var cappedType = GetHtmlType(column.DotNetType);
            cappedType = char.ToUpper(cappedType[0]) + cappedType[1..];
            //If the data type in SQL is Text, then use TextArea rather than Input'
            if (column.DataType == "text") cappedType = "TextArea";

            inputTag += "<div class='col-2'>\n<label for = '" + column.Name + "'>" +
                        column.Name + ":</label>\n</div>\n<div class='col-4'>\n<Input" +
                        cappedType + " @bind-Value = " + ToChar(34) + className?.ToLower() + "." +
                        column.Name + ToChar(34) + " class='form-control'";

            if (column.DataType.Contains("varchar") || column.DataType.Contains("text"))
            {
                inputTag += " style='width:100%;'";
            }

            //The date ranges don't work in the model, so adding here.
            if (column.DataType.Contains("date"))
            {
                inputTag += $" min={ToChar(34)}1753-01-01{ToChar(34)} max={ToChar(34)}9999-12-31{ToChar(34)}";
            }

            inputTag += " id = '" + column.Name + "'/></div>\n";
        }
        return inputTag;
    }
    
    public static string GetSqlSearchStatement(this Column column)
    {
        var sqlSearch = "";
        if (column.DataType.Contains("date"))
        {
            sqlSearch += $"CONVERT(varchar(12),{column.Name},101) LIKE + '%' + @param + '%' OR ";
        }

        //convert numbers to string for like search
        if (column.DataType.Contains("int") || column.DataType.Contains("decimal") ||
            column.DataType.Contains("money"))
        {
            sqlSearch += $"CAST({column.Name} AS varchar(20)) LIKE '%' + @param + '%' OR ";
        }

        //No conversion for text fields
        if (column.DataType.Contains("text") || column.DataType.Contains("char"))
        {
            sqlSearch += column.Name + " LIKE '%' + @param + '%' OR ";
        }

        return sqlSearch;
    }
    
    public static string GetModelProperties(this Column column, string primaryKey)
    {
        var modelProperties = Empty;
        if (column.IsRequired && column.Name != primaryKey) modelProperties += "[Required]\n";
        if (column.Length > 0) modelProperties += $"[StringLength({column.Length})]\n";
        modelProperties += $"public {column.DotNetType} {column.Name} {{ get; set; }}\n";
        return modelProperties;
    }
    
    public static string GetSqlDateSearchWhereClause(this Column column, int dateCounter)
    {
        //convert dates to string for Like search
        if (!column.DataType.Contains("date")) return Empty;
        //Isolate the first date or datetime field
        dateCounter += 1;
        return dateCounter == 1 ? $" CAST({column.Name} AS Date) Between @startDate AND @endDate" : Empty;
    }
    
}