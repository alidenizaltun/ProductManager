using Microsoft.AspNetCore.Mvc;

namespace ProductManager.WebUI.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
