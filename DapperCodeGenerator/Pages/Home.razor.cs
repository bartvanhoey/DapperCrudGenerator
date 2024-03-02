using DapperCodeGenerator.Models;
using DapperCodeGenerator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DapperCodeGenerator.Pages
{
    public class HomeBase : ComponentBase
    {
        protected readonly CodeTemplate? CodeTemplate = new();

        [Inject]
        public ITableInfoService? CodeTemplateProcessor { get; set; }
        
        public async Task GenerateCode(EditContext editContext) 
            => await CodeTemplateProcessor!.Process(CodeTemplate);
    }
}