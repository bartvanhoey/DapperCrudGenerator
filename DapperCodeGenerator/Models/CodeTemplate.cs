using Pluralize.NET.Core;

namespace DapperCodeGenerator.Models
{
    public class CodeTemplate
    {
        public string? NamespaceName { get; set; }
        public string? CreateTableStatement { get; set; }
        public string? ClassName { get; set; }
        public string? ObjectName { get; set; }
        public string? ObjectList { get; set; }
        public string? RawTemplate { get; set; }
        public string? GeneratedCode { get; set; }
    }

    public static class CodeTemplateExtensions
    {
        public static string? GetTableName(this CodeTemplate codeTemplate)
        {
            var createTableStatement = codeTemplate.CreateTableStatement;
            if (createTableStatement == null || !createTableStatement.Contains("CREATE TABLE [dbo].[")) return null;
            var tableInfo = createTableStatement.Replace("CREATE TABLE [dbo].[", "");
            return tableInfo[..tableInfo.IndexOf("]", StringComparison.Ordinal)].Replace("\n", "");
        }

        public static string GetTableInfo(this CodeTemplate codeTemplate)
        {
            var createTableStatement = codeTemplate.CreateTableStatement;
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
        
        public static string? GetClassName(this CodeTemplate codeTemplate)
        {
            var tableName = codeTemplate.GetTableName();
            if (tableName == null) return null;
            var charArray = tableName.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            var className = new string(charArray);
            if (className[..3] == "Tbl") className = className[3..];
            return className;
        }


        public static string? GetListName(this CodeTemplate codeTemplate)
        {
            var className = codeTemplate.GetClassName();
            if (className == null) return null;
            var objectName = className.ToLower();
            var listName = new Pluralizer().Pluralize(objectName);
            if (listName == objectName) listName += "s";
            return listName;
        }
    }
    
}