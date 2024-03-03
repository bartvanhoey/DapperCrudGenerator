using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public class CodeGenerator(HttpClient httpClient) : ICodeGenerator
{
    private Replacer? Replacer { get; set; }

    private void SetReplacer(Replacer? replacer) => Replacer = replacer;
    public async Task<CodeGeneratorResult> Generate(Replacer replacer)
    {


        return new CodeGeneratorResult()
        {
            AddNugetPackages = await AddNugetPackages(),
            SqlConnectionString = await SqlConnectionString(),
            SqlConnectionConfiguration = await SqlConnectionConfiguration(),
            RegisterServices = await RegisterServices(),
            ModelClass = await ModelClass(),
            ServiceInterface = await CrudServiceInterface(),
            ServiceClass = await CrudServiceClass(),

            InsertStoredProcedure = await InsertStoredProcedure(),
            UpdateStoredProcedure = await UpdateStoredProcedure(),
            SelectStoredProcedure = await SelectStoredProcedure(),
            SelectOneStoredProcedure = await SelectOneStoredProcedure(),
            SelectLikeStoredProcedure = await SelectLikeStoredProcedure(),
            SelectDataRangeStoredProcedure = await SelectDataRangeStoredProcedure(),
            DeleteStoredProcedure = await DeleteStoredProcedure(),


            AddEditRazorPage = await AddEditRazorPage(),
            ListRazorPage = await ListRazorPage(),
            ReportRazorPage = await ReportRazorPage(),
            DeleteRazorPage = await DeleteRazorPage(),
            NavLinkToList = await NavLinkToList(),
            CssStyling = await CssStyling()
        };
    }

    private async Task<string> AddNugetPackages()
        => await ReplacePlaceholders("AddNugetPackages.txt");

    private async Task<string> SqlConnectionString()
        => await ReplacePlaceholders("SqlConnectionStrings.txt");

    private async Task<string> SqlConnectionConfiguration() 
        => await ReplacePlaceholders("SqlConnectionConfiguration.txt");

    private async Task<string> InsertStoredProcedure() 
        => await ReplacePlaceholders("InsertStoredProcedure.txt");

    private async Task<string> UpdateStoredProcedure()
        => await ReplacePlaceholders("UpdateStoredProcedure.txt");

    private async Task<string> SelectStoredProcedure()
        => await ReplacePlaceholders("SelectStoredProcedure.txt");

    private async Task<string> SelectOneStoredProcedure()
        => await ReplacePlaceholders("SelectOneStoredProcedure.txt");

    private async Task<string> SelectLikeStoredProcedure()
        => await ReplacePlaceholders("SelectLikeStoredProcedure.txt");

    private async Task<string> SelectDataRangeStoredProcedure()
        => await ReplacePlaceholders("SelectDataRangeStoredProcedure.txt");

    private async Task<string> DeleteStoredProcedure()
        => await ReplacePlaceholders("DeleteStoredProcedure.txt");

    private async Task<string> ModelClass()
        => await ReplacePlaceholders("ModelClass.txt");

    private async Task<string> CrudServiceInterface()
        => await ReplacePlaceholders("CrudServiceInterface.txt");
    

    private async Task<string> CrudServiceClass()
        => await ReplacePlaceholders("CrudServiceClass.txt");

    private async Task<string> AddEditRazorPage()
        => await ReplacePlaceholders("RazorPageAddEdit.txt");

    private async Task<string> ListRazorPage()
        => await ReplacePlaceholders("RazorPageList.txt");

    private async Task<string> ReportRazorPage()
        => await ReplacePlaceholders("RazorPageReport.txt");

    private async Task<string> DeleteRazorPage()
        => await ReplacePlaceholders("RazorPageDelete.txt");

    private async Task<string> NavLinkToList()
        => await ReplacePlaceholders("NavLinkToList.txt");

    private async Task<string> CssStyling()
        => await ReplacePlaceholders("CssStyling.txt");

    private async Task<string> RegisterServices()
        => await ReplacePlaceholders("ServicesRegistration.txt");
    

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