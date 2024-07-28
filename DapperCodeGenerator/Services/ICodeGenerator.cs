using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface ICodeGenerator
{
    Task<CodeGeneratorResult> Generate(Replacer replacer);
}