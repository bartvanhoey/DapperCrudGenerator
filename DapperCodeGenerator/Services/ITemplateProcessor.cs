using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface ITemplateProcessor
{
    Task Process(Template? template);
}