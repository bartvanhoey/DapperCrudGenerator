using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface IFieldNameNormalizer
{
    Task<List<Tablefield>> NormalizeFieldNames(string rawText, string objectName, string tableName);
}