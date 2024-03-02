using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface ITableInfoService
{
    Task Process(CodeTemplate? codeTemplate);
}