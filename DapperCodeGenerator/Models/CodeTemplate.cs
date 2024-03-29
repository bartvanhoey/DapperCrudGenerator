namespace DapperCodeGenerator.Models
{
    public class CodeTemplate
    {
        public string? NamespaceName { get; set; }
        public string? CreateTableStatement { get; set; }
        public string? ClassName { get; set; }
        public string? ObjectName { get; set; }
        public string? ObjectList { get; set; }
        public string? RawTemplate { get; set; }
        public string? GeneratedCode { get; set; }
    }
}