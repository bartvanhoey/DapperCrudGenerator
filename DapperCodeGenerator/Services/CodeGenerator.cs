using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public class CodeGenerator(HttpClient httpClient) : ICodeGenerator
{
    private Replacer? Replacer { get; set; }

    public void SetReplacer(Replacer? replacer) => Replacer = replacer;
    public async Task<string> SqlConnectionConfiguration() 
        => await ReplacePlaceholders("SqlConnectionConfiguration.txt");

    public async Task<string> InsertStoredProcedure() 
        => await ReplacePlaceholders("InsertStoredProcedure.txt");

    public async Task<string> UpdateStoredProcedure()
        => await ReplacePlaceholders("UpdateStoredProcedure.txt");

    public async Task<string> SelectStoredProcedure()
        => await ReplacePlaceholders("SelectStoredProcedure.txt");

    public async Task<string> SelectOneStoredProcedure()
        => await ReplacePlaceholders("SelectOneStoredProcedure.txt");

    public async Task<string> SelectLikeStoredProcedure()
        => await ReplacePlaceholders("SelectLikeStoredProcedure.txt");

    public async Task<string> SelectDataRangeStoredProcedure()
        => await ReplacePlaceholders("SelectDataRangeStoredProcedure.txt");

    public async Task<string> DeleteStoredProcedure()
        => await ReplacePlaceholders("DeleteStoredProcedure.txt");

    public async Task<string> ModelClass()
        => await ReplacePlaceholders("ModelClass.txt");

    public async Task<string> CrudServiceInterface()
        => await ReplacePlaceholders("CrudServiceInterface.txt");
    

    public async Task<string> CrudServiceClass()
        => await ReplacePlaceholders("CrudServiceClass.txt");

    public async Task<string> AddEditRazorPage()
        => await ReplacePlaceholders("RazorPageAddEdit.txt");

    public async Task<string> ListRazorPage()
        => await ReplacePlaceholders("RazorPageList.txt");

    public async Task<string> ReportRazorPage()
        => await ReplacePlaceholders("RazorPageReport.txt");

    public async Task<string> DeleteRazorPage()
        => await ReplacePlaceholders("RazorPageDelete.txt");

    public async Task<string> NavLinkToList()
        => await ReplacePlaceholders("NavLinkToList.txt");

    public async Task<string> CssStyling()
        => await ReplacePlaceholders("CssStyling.txt");

    private async Task<string> ReplacePlaceholders(string fileName)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        if (Replacer == null) throw new ArgumentException("Replace is null");
          
        var template = await httpClient.GetStringAsync(fileName);
          
        template = template.Replace("@@PK", Replacer.PrimaryKey);
        template = template.Replace("@@spOneParam", Replacer.SprocOneParam);
        template = template.Replace("@@spAllParams", Replacer.SprocAllParams);
        template = template.Replace("@@spInsertParams", Replacer.SprocInsertParams);
        template = template.Replace("@@OBJECTNAME", Replacer.ObjectName);
        template = template.Replace("@@MODELCODE", Replacer.ModelCode);
        template = template.Replace("@@PARAMETERADDUPDATEONLY", Replacer.ParametersAddUpdateOnly);
        template = template.Replace("@@PARAMETERSADD", Replacer.ParametersAdd);
        template = template.Replace("@@PRIMARYKEY", Replacer.PrimaryKey);
        template = template.Replace("@@INPUTONEROWS", Replacer.InputOneRows);
        template = template.Replace("@@FIELDNAMESTABLEHEADING", Replacer.FieldNamesTableHeading);
        template = template.Replace("@@FIELDNAMESTABLEROW", Replacer.FieldNamesTableRow);
        template = template.Replace("@@CONFIRMDELETEROWS", Replacer.ConfirmDeleteRows);
        template = template.Replace("@@SQLINSERT",  Replacer.SqlInsert);
        template = template.Replace("@@SQLUPDATE" ,Replacer.SqlUpdate);
        template = template.Replace("@@SQLSEARCH",  Replacer.SqlSearch );
        template = template.Replace("@@SQLSELECTLIST", Replacer.SqlSelectList);
        template = template.Replace("@@SQLSELECTONE", Replacer.SqlSelectOne);
        template = template.Replace("@@SQLDELETE",  Replacer.SqlDelete);
        template = template.Replace("@@SQLDATERANGE", Replacer.SqlDateRange );
        template = template.Replace("@@TABLENAME", Replacer.TableName);
        template = template.Replace("@@OBJECT", Replacer.ObjectName);
        template = template.Replace("@@NAMESPACE", Replacer.NameSpaceName);
        template = template.Replace("@@CLASSNAME",  Replacer.ClassName);
        template = template.Replace("@@CLASSService", Replacer.ServiceName);
        template = template.Replace("@@objectname", Replacer.ObjectName);
        template = template.Replace("@@LISTPAGENAME", Replacer.ObjectName + "list");
        template = template.Replace("@@REPORTPAGENAME",  Replacer.ObjectName + "report");
        template = template.Replace("@@objectlist", Replacer.ListName);
        template = template.Replace("@@listpagename", Replacer.ListPageName);
        return template;
    }


     
}