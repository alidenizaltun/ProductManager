using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.WebUI.Models.ProductOperations;

namespace ProductManagement.WebUI.Controllers;

public sealed class AttributesController : Controller
{
    private readonly IProductOperationsService _service;

    public AttributesController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        SetBreadcrumb("Attribute Tanimlari");
        var viewModel = await BuildIndexPageViewModelAsync(cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Attribute");
        ViewData["Title"] = "Yeni Attribute";
        return View("Edit", new AttributeDefinitionFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttributeDefinitionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Attribute");
            ViewData["Title"] = "Yeni Attribute";
            return View("Edit", model);
        }

        await _service.CreateAttributeDefinitionAsync(new CreateProductAttributeDefinitionRequestDto
        {
            Key = model.Key.Trim(),
            DisplayName = model.DisplayName.Trim(),
            DataType = model.DataType,
            IsRequired = model.IsRequired,
            IsFilterable = model.IsFilterable,
            IsVariantAxis = model.IsVariantAxis,
            AllowedValuesJson = null,
            ValidationRuleJson = null
        }, cancellationToken);

        TempData["Success"] = "Attribute olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal([Bind(Prefix = nameof(AttributeDefinitionListPageViewModel.CreateModal))] AttributeDefinitionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Attribute Tanimlari");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreateAttributeDefinitionAsync(new CreateProductAttributeDefinitionRequestDto
        {
            Key = model.Key.Trim(),
            DisplayName = model.DisplayName.Trim(),
            DataType = model.DataType,
            IsRequired = model.IsRequired,
            IsFilterable = model.IsFilterable,
            IsVariantAxis = model.IsVariantAxis,
            AllowedValuesJson = null,
            ValidationRuleJson = null
        }, cancellationToken);

        TempData["Success"] = "Attribute olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var attribute = await _service.GetAttributeDefinitionByIdAsync(id, cancellationToken);
        if (attribute is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Attribute Duzenle");
        ViewData["Title"] = "Attribute Duzenle";

        return View(new AttributeDefinitionFormViewModel
        {
            Id = attribute.Id,
            Key = attribute.Key,
            DisplayName = attribute.DisplayName,
            DataType = attribute.DataType,
            IsRequired = attribute.IsRequired,
            IsFilterable = attribute.IsFilterable,
            IsVariantAxis = attribute.IsVariantAxis,
            AllowedValuesJson = attribute.AllowedValuesJson,
            ValidationRuleJson = attribute.ValidationRuleJson
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AttributeDefinitionFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Attribute Duzenle");
            ViewData["Title"] = "Attribute Duzenle";
            return View(model);
        }

        var updated = await _service.UpdateAttributeDefinitionAsync(id, new UpdateProductAttributeDefinitionRequestDto
        {
            Key = model.Key.Trim(),
            DisplayName = model.DisplayName.Trim(),
            DataType = model.DataType,
            IsRequired = model.IsRequired,
            IsFilterable = model.IsFilterable,
            IsVariantAxis = model.IsVariantAxis,
            AllowedValuesJson = null,
            ValidationRuleJson = null
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Attribute guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal([Bind(Prefix = nameof(AttributeDefinitionListPageViewModel.EditModal))] AttributeDefinitionFormViewModel model, CancellationToken cancellationToken)
    {
        Guid attributeId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            attributeId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(AttributeDefinitionListPageViewModel.EditModal)}.{nameof(AttributeDefinitionFormViewModel.Id)}", "Attribute bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Attribute Tanimlari");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdateAttributeDefinitionAsync(attributeId, new UpdateProductAttributeDefinitionRequestDto
        {
            Key = model.Key.Trim(),
            DisplayName = model.DisplayName.Trim(),
            DataType = model.DataType,
            IsRequired = model.IsRequired,
            IsFilterable = model.IsFilterable,
            IsVariantAxis = model.IsVariantAxis,
            AllowedValuesJson = null,
            ValidationRuleJson = null
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Attribute guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAttributeDefinitionAsync(id, cancellationToken);
        TempData["Success"] = "Attribute silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AttributeDefinitionListPageViewModel> BuildIndexPageViewModelAsync(
        CancellationToken cancellationToken,
        AttributeDefinitionFormViewModel? createModal = null,
        AttributeDefinitionFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false)
    {
        var attributes = await _service.GetAttributeDefinitionsAsync(cancellationToken);

        return new AttributeDefinitionListPageViewModel
        {
            Attributes = attributes,
            CreateModal = createModal ?? new AttributeDefinitionFormViewModel(),
            EditModal = editModal ?? new AttributeDefinitionFormViewModel(),
            OpenCreateModal = openCreateModal,
            OpenEditModal = openEditModal
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
