using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public interface ICodeGenerator
{
     
     Task<string> SqlConnectionConfiguration();
     void SetReplacer(Replacer? replacer);
     Task<string> InsertStoredProcedure();
     Task<string> UpdateStoredProcedure();
     Task<string> SelectStoredProcedure();
     Task<string> SelectOneStoredProcedure();
     Task<string> SelectLikeStoredProcedure();
     Task<string> SelectDataRangeStoredProcedure();
     Task<string> DeleteStoredProcedure();
     Task<string> ModelClass();
     Task<string> CrudServiceInterface();
     Task<string> CrudServiceClass();
     Task<string> AddEditRazorPage();
     Task<string> ListRazorPage();
     Task<string> ReportRazorPage();
     Task<string> DeleteRazorPage();
     Task<string> NavLinkToList();
     Task<string> CssStyling();
}