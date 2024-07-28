using System.Text.RegularExpressions;

namespace DapperCodeGenerator.Utils;

public static class Util
{
    public static int GetColumnLength(string fieldName)
    {
        var dataType = GetDataType(fieldName);
        if (!dataType.Contains("char")) return 0;
        var fieldNameLength = KeepNumbersOnly(dataType);
        if (int.TryParse(fieldNameLength, out var number)) ;
        return number;
    }

    public static string GetColumnName(string fieldName)
    {
        var columnName = fieldName.Substring(1, fieldName.IndexOf(']') - 1);
        return char.ToUpper(columnName[0]) + columnName[1..];
    }

    public static string KeepAlphaCharactersOnly(string input) =>
        Regex.Replace(input, @"[^a-zA-Z\._]", string.Empty);

    private static string KeepNumbersOnly(string input) => Regex.Replace(input, @"[^0-9.]", string.Empty);

    public static string GetDataType(string fieldName)
    {
        var datatype = fieldName[(fieldName.IndexOf(']') + 2)..];
        datatype = datatype.Replace("IDENTITY(1,1) ", "");
        datatype = datatype.Replace("IDENTITY (1, 1) ", "");
        datatype = datatype.Replace("NULL", "").Replace("NOT", "");
        datatype = datatype.Replace("[", "").Replace("]", "").Trim();
        return datatype;
    }
}