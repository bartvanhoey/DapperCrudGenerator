using System.Text.RegularExpressions;
using static System.String;
using static DapperCodeGenerator.Utils.Util;

namespace DapperCodeGenerator.Services
{
    public static class TypeMapper
    {
        private static readonly Dictionary<string, string> SqlDotNetTypes = new()
        {
            { "bigint", "int" },
            { "binary", "Byte[]" },
            { "bit", "bool" },
            { "char", "string" },
            { "date", "DateTime" },
            { "datetime", "DateTime" },
            { "datetime2", "DateTime" },
            { "datetimeoffset", "DateTimeOffset" },
            { "decimal", "decimal" },
            { "float", "double" },
            { "image", "byte[]" },
            { "int", "int" },
            { "money", "decimal" },
            { "nchar", "string" },
            { "ntext", "string" },
            { "numeric", "decimal" },
            { "nvarchar", "string" },
            { "nvarcharmax", "string" },
            { "real", "decimal" },
            { "rowversion", "byte[]" },
            { "smalldatetime", "DateTime" },
            { "smallint", "int" },
            { "smallmoney", "decimal" },
            { "sql_variant", "object" },
            { "text", "string" },
            { "time", "TimeSpan" },
            { "timestamp", "byte[]" },
            { "tinyint", "int" },
            { "uniqueidentifier", "string" },
            { "varbinary", "byte[]" },
            { "varbinarymax", "byte[]" },
            { "varchar", "string" },
            { "varcharmax", "string" },
            { "xml", "string" },
        };

        private static string GetAlphaCharactersOnly(string input) =>
            Regex.Replace(input, @"[^a-zA-Z\._]", Empty);

        public static string? GetDotNetType(string fieldName)
            => SqlDotNetTypes.GetValueOrDefault(GetAlphaCharactersOnly(GetDataType(fieldName)));

        private static readonly Dictionary<string, string> HtmlTypes = new()
        {
            { "byte", "text" },
            { "byte[]", "file" },
            { "int", "number" },
            { "float", "number" },
            { "decimal", "number" },
            { "double", "number" },
            { "bool", "checkbox" },
            { "string", "text" },
            { "Date", "date" },
            { "DateTime", "date" },
            { "DateTimeOffset", "datetime-local" },
            { "image", "image" },
            { "object", "file" },
            { "TimeSpan", "time" },
            { "TimeStamp", "time" },
            { "xml", "string" }
        };

        public static string? GetHtmlType(string? dotnetType) =>
            IsNullOrWhiteSpace(dotnetType) ? "" : HtmlTypes.GetValueOrDefault(dotnetType);

        private static readonly Dictionary<string, string> DbTypes = new()
        {
            { "bigint", "Int64" },
            { "binary", "Binary" },
            { "bit", "Boolean" },
            { "char", "Char" },
            { "date", "Date" },
            { "datetime", "DateTime" },
            { "datetime2", "DateTime2" },
            { "datetimeoffset", "DateTimeOffset" },
            { "decimal", "Decimal" },
            { "float", "Decimal" },
            { "image", "Binary" },
            { "int", "Int32" },
            { "money", "Decimal" },
            { "nchar", "String" },
            { "ntext", "String" },
            { "numeric", "Decimal" },
            { "nvarchar", "String" },
            { "nvarcharmax", "String" },
            { "real", "Decimal" },
            { "rowversion", "Timestamp" },
            { "smalldatetime", "DateTime" },
            { "smallint", "Int16" },
            { "smallmoney", "Decimal" },
            { "sql_variant", "Object" },
            { "text", "String" },
            { "time", "Time" },
            { "timestamp", "Object" },
            { "tinyint", "Int16" },
            { "uniqueidentifier", "Guid" },
            { "varbinary", "Binary" },
            { "varbinarymax", "Binary" },
            { "varchar", "String" },
            { "varcharmax", "String" },
            { "xml", "Xml" },
        };

        public static string? GetDbType(string fieldName)
            => DbTypes.GetValueOrDefault(GetAlphaCharactersOnly(GetDataType(fieldName)));
    }
}