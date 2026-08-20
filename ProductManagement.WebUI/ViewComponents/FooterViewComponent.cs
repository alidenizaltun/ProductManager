using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.WebUI.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
