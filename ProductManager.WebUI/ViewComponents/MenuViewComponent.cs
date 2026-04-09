using Microsoft.AspNetCore.Mvc;

namespace ProductManager.WebUI.Areas.ViewComponents;

public class MenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
