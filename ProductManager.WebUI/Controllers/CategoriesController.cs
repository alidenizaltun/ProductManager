using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.WebUI.Models.ProductOperations;

namespace ProductManager.WebUI.Controllers;

public sealed class CategoriesController : Controller
{
    private readonly IProductOperationsService _service;

    public CategoriesController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        SetBreadcrumb("Kategoriler");
        var viewModel = await BuildIndexPageViewModelAsync(cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Kategori");
        ViewData["Title"] = "Yeni Kategori";
        return View("Edit", new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Kategori");
            ViewData["Title"] = "Yeni Kategori";
            return View("Edit", model);
        }

        await _service.CreateCategoryAsync(new CreateProductCategoryRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            ParentCategoryId = model.ParentCategoryId
        }, cancellationToken);

        TempData["Success"] = "Kategori olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal([Bind(Prefix = nameof(CategoryListPageViewModel.CreateModal))] CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Kategoriler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreateCategoryAsync(new CreateProductCategoryRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            ParentCategoryId = model.ParentCategoryId
        }, cancellationToken);

        TempData["Success"] = "Kategori olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var category = await _service.GetCategoryByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Kategori Duzenle");
        ViewData["Title"] = "Kategori Duzenle";

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Kategori Duzenle");
            ViewData["Title"] = "Kategori Duzenle";
            return View(model);
        }

        var updated = await _service.UpdateCategoryAsync(id, new UpdateProductCategoryRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            ParentCategoryId = model.ParentCategoryId
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Kategori guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal([Bind(Prefix = nameof(CategoryListPageViewModel.EditModal))] CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        Guid categoryId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            categoryId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(CategoryListPageViewModel.EditModal)}.{nameof(CategoryFormViewModel.Id)}", "Kategori bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Kategoriler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdateCategoryAsync(categoryId, new UpdateProductCategoryRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            ParentCategoryId = model.ParentCategoryId
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Kategori guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteCategoryAsync(id, cancellationToken);
        TempData["Success"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CategoryListPageViewModel> BuildIndexPageViewModelAsync(
        CancellationToken cancellationToken,
        CategoryFormViewModel? createModal = null,
        CategoryFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false)
    {
        var categories = await _service.GetCategoriesAsync(cancellationToken);

        return new CategoryListPageViewModel
        {
            Categories = categories,
            CreateModal = createModal ?? new CategoryFormViewModel(),
            EditModal = editModal ?? new CategoryFormViewModel(),
            OpenCreateModal = openCreateModal,
            OpenEditModal = openEditModal
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
