namespace DapperCodeGenerator.Models;

public class Replacer
{
    public string SprocAllParams { get; init; } = "";
    public string SprocInsertParams { get; init; } = "";
    public string PrimaryKey { get; init; } = "";
    public string SprocOneParam { get; init; } = "";
    public string? ObjectName { get; init; } = "";
    public string ModelCode { get; init; } = "";
    public string ParametersAddUpdateOnly { get; init; } = "";
    public string ConfirmDeleteRows { get; init; } = "";
    public string ParametersAdd { get; init; } = "";
    public string InputOneRows { get; init; } = "";
    public string FieldNamesTableHeading { get; init; } = "";
    public string FieldNamesTableRow { get; init; } = "";
    public string SqlUpdate { get; init; } = "";
    public string SqlInsert { get; init; } = "";
    public string SqlSelectList { get; init; } = "";
    public string SqlSelectOne { get; init; } = "";
    public string SqlSearch { get; init; } = "";
    public string SqlDelete { get; init; } = "";
    public string SqlDateRange { get; init; } = "";
    public string? TableName { get; init; } = "";
    public string ServiceName { get; init; }= "";
    public string ListPageName { get; init; }= "";
    public string? ListName { get; init; }= "";
    public string? FieldList { get; init; }= "";
    public string? NameSpaceName { get; init; }= "";
    public string? ClassName { get; init; } = "";
}