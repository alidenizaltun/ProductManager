using Microsoft.AspNetCore.Mvc;

namespace ProductManager.WebUI.Areas.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
