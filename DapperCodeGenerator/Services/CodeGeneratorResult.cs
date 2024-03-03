namespace DapperCodeGenerator.Services;

public class CodeGeneratorResult
{
    public string? AddNugetPackages { get; set; }
    public string? SqlConnectionString { get; set; }
    public string? SqlConnectionConfiguration { get; set; }
    public string? InsertStoredProcedure { get; set; }
    public string? UpdateStoredProcedure { get; set; }
    public string? SelectStoredProcedure { get; set; }
    public string? SelectOneStoredProcedure { get; set; }
    public string? SelectLikeStoredProcedure { get; set; }
    public string? SelectDataRangeStoredProcedure { get; set; }
    public string? DeleteStoredProcedure { get; set; }
    public string? ModelClass { get; set; }
    public string? ServiceInterface { get; set; }
    public string? ServiceClass { get; set; }
    public string? AddEditRazorPage { get; set; }
    public string? ListRazorPage { get; set; }
    public string? ReportRazorPage { get; set; }
    public string? DeleteRazorPage { get; set; }
    public string? NavLinkToList { get; set; }
    public string? CssStyling { get; set; }
    public string? RegisterServices { get; set; }
}