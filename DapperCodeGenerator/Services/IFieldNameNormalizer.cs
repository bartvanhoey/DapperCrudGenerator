using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface IFieldNameNormalizer
{
    Task<List<TableField>> NormalizeFieldNames(string rawText, string objectName, string tableName);
}