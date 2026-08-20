using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.WebUI.Areas.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
