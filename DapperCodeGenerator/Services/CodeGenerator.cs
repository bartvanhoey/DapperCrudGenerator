using DapperCodeGenerator.Models;

namespace DapperCodeGenerator.Services;

public class CodeGenerator(HttpClient httpClient) : ICodeGenerator
{
    private Replacer? Replacer { get; set; }

    public async Task<CodeGeneratorResult> Generate(Replacer replacer)
    {
        Replacer = replacer;

        return new CodeGeneratorResult
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

        template = template.Replace("@@PrimaryKey", Replacer.PrimaryKey);
        template = template.Replace("@@sprocOneParam", Replacer.SprocOneParam);
        template = template.Replace("@@sprocAllParams", Replacer.SprocAllParams);
        template = template.Replace("@@sprocInsertParams", Replacer.SprocInsertParams);
        template = template.Replace("@@ObjectName", Replacer.ObjectName);
        template = template.Replace("@@ModelCode", Replacer.ModelCode);
        template = template.Replace("@@ParameterAddUpdateOnly", Replacer.ParametersAddUpdateOnly);
        template = template.Replace("@@ParametersAdd", Replacer.ParametersAdd);
        template = template.Replace("@@InputOneRows", Replacer.InputOneRows);
        template = template.Replace("@@FieldNamesStableHeading", Replacer.FieldNamesTableHeading);
        template = template.Replace("@@FieldNamesStableRow", Replacer.FieldNamesTableRow);
        template = template.Replace("@@ConfirmDeleteRows", Replacer.ConfirmDeleteRows);
        template = template.Replace("@@SqlInsert", Replacer.SqlInsert);
        template = template.Replace("@@SqlUpdate", Replacer.SqlUpdate);
        template = template.Replace("@@SqlSearch", Replacer.SqlSearch);
        template = template.Replace("@@SqlSelectList", Replacer.SqlSelectList);
        template = template.Replace("@@SqlSelectOne", Replacer.SqlSelectOne);
        template = template.Replace("@@SqlDelete", Replacer.SqlDelete);
        template = template.Replace("@@SqlDateRange", Replacer.SqlDateRange);
        template = template.Replace("@@TableName", Replacer.TableName);
        template = template.Replace("@@NameSpace", Replacer.NameSpaceName);
        template = template.Replace("@@ClassName", Replacer.ClassName);
        template = template.Replace("@@ClassService", Replacer.ServiceName);
        template = template.Replace("@@ObjectName", Replacer.ObjectName);
        template = template.Replace("@@ListPageName", Replacer.ObjectName + "list");
        template = template.Replace("@@ReportPageName", Replacer.ObjectName + "report");
        template = template.Replace("@@ObjectList", Replacer.ListName);
        template = template.Replace("@@ListPageName", Replacer.ListPageName);
        return template;
        
    }
}