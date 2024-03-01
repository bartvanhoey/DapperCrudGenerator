using DapperCodeGenerator.Models;
using DapperCodeGenerator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Pluralize.NET.Core;

namespace DapperCodeGenerator.Pages
{
    public class HomeBase : ComponentBase
    {
        protected readonly CodeTemplate? CodeTemplate = new();

        [Inject]
        public IFieldNameNormalizer? FieldNormalizer { get; set; }
        
        public async Task GenerateCode(EditContext editContext)
        {
            if (CodeTemplate == null) return;
            var nameSpaceName = CodeTemplate.NamespaceName;
            var tableInfo = CodeTemplate.TableDesign;
            if (tableInfo == null || !tableInfo.Contains("CREATE TABLE [dbo].[")) return;
            tableInfo = tableInfo.Replace("CREATE TABLE [dbo].[", "");
            var lastIndexClosingBracket = tableInfo.LastIndexOf(")");
            if (lastIndexClosingBracket > 0) tableInfo = tableInfo[..(lastIndexClosingBracket + 1)];
            var lastIndexConstraint = tableInfo.IndexOf("CONSTRAINT");
            if (lastIndexConstraint > 0) tableInfo = tableInfo[..lastIndexConstraint];


            tableInfo = tableInfo.Trim();
            var tableName = tableInfo.Substring(0, tableInfo.IndexOf("]")).Replace("\n", "");
            char[] a = tableName.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            var className = new string(a);
            if (className[..3] == "Tbl")
            {
                className = className[3..];
            }

            string classService = className + "Service";
            var objectName = className.ToLower();
            var listPageName = objectName + "list";
            string listName = new Pluralizer().Pluralize(objectName);
            if (listName == objectName) listName += "s";

            var fieldList = @CodeTemplate.TableDesign;
            int firstParentheses = tableInfo.IndexOf("(");
            tableInfo = tableInfo.Substring(firstParentheses + 1);
            tableInfo = tableInfo.Replace("\n", "").Replace("\t", "");


             
            var normalizeFieldNames = await FieldNormalizer!
                    .NormalizeFieldNames(tableInfo, objectName, tableName);


            Console.WriteLine(tableInfo);

        }


    }
}