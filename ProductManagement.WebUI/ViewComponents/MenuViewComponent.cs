using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.WebUI.Areas.ViewComponents;

public class MenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
