using Microsoft.AspNetCore.Mvc;
using P3AddNewFunctionalityDotNetCore.Models.Services;

namespace P3AddNewFunctionalityDotNetCore.Components
{
    public class LanguageSelectorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ILanguageService languageService)
        {
            return View(languageService);
        }
    }
}
