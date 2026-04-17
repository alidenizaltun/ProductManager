using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        SetBreadcrumb("Yeni Kategori");
        ViewData["Title"] = "Yeni Kategori";

        var model = new CategoryFormViewModel();
        await PopulateParentCategoryOptionsAsync(model, cancellationToken);
        return View("Edit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Kategori");
            ViewData["Title"] = "Yeni Kategori";

            await PopulateParentCategoryOptionsAsync(model, cancellationToken);
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

        var model = new CategoryFormViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId
        };

        await PopulateParentCategoryOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Kategori Duzenle");
            ViewData["Title"] = "Kategori Duzenle";

            await PopulateParentCategoryOptionsAsync(model, cancellationToken);
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
        var createModalModel = createModal ?? new CategoryFormViewModel();
        var editModalModel = editModal ?? new CategoryFormViewModel();

        createModalModel.ParentCategoryOptions = BuildParentCategoryOptions(
            categories,
            createModalModel.ParentCategoryId,
            excludedCategoryId: null);

        editModalModel.ParentCategoryOptions = BuildParentCategoryOptions(
            categories,
            editModalModel.ParentCategoryId,
            excludedCategoryId: editModalModel.Id);

        return new CategoryListPageViewModel
        {
            Categories = categories,
            CreateModal = createModalModel,
            EditModal = editModalModel,
            OpenCreateModal = openCreateModal,
            OpenEditModal = openEditModal
        };
    }

    private async Task PopulateParentCategoryOptionsAsync(CategoryFormViewModel model, CancellationToken cancellationToken)
    {
        var categories = await _service.GetCategoriesAsync(cancellationToken);
        model.ParentCategoryOptions = BuildParentCategoryOptions(categories, model.ParentCategoryId, model.Id);
    }

    private static IReadOnlyList<SelectListItem> BuildParentCategoryOptions(
        IReadOnlyList<ProductCategoryDto> categories,
        Guid? selectedCategoryId,
        Guid? excludedCategoryId)
    {
        var items = new List<SelectListItem>
        {
            new("Üst kategori yok", string.Empty, !selectedCategoryId.HasValue)
        };

        foreach (var category in categories
                     .Where(x => x.Id != excludedCategoryId)
                     .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            items.Add(new SelectListItem(
                $"{category.Name} ({category.Code})",
                category.Id.ToString(),
                selectedCategoryId == category.Id));
        }

        return items;
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
