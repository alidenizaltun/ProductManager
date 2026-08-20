using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.WebUI.Models.ProductOperations;

namespace ProductManagement.WebUI.Controllers;

public sealed class WarehousesController : Controller
{
    private readonly IProductOperationsService _service;

    public WarehousesController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool includeInactive, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Depolar");
        var viewModel = await BuildIndexPageViewModelAsync(includeInactive, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Depo");
        ViewData["Title"] = "Yeni Depo";
        return View("Edit", new WarehouseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Depo");
            ViewData["Title"] = "Yeni Depo";
            return View("Edit", model);
        }

        await _service.CreateWarehouseAsync(new CreateWarehouseRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            IsActive = model.IsActive
        }, cancellationToken);

        TempData["Success"] = "Depo olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal([Bind(Prefix = nameof(WarehouseListPageViewModel.CreateModal))] WarehouseFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Depolar");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreateWarehouseAsync(new CreateWarehouseRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            IsActive = model.IsActive
        }, cancellationToken);

        TempData["Success"] = "Depo olusturuldu.";
        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _service.GetWarehouseByIdAsync(id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Depo Duzenle");
        ViewData["Title"] = "Depo Duzenle";

        return View(new WarehouseFormViewModel
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Description = warehouse.Description,
            Address = warehouse.Address,
            City = warehouse.City,
            Country = warehouse.Country,
            IsActive = warehouse.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, WarehouseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Depo Duzenle");
            ViewData["Title"] = "Depo Duzenle";
            return View(model);
        }

        var updated = await _service.UpdateWarehouseAsync(id, new UpdateWarehouseRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            IsActive = model.IsActive
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Depo guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal([Bind(Prefix = nameof(WarehouseListPageViewModel.EditModal))] WarehouseFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        Guid warehouseId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            warehouseId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(WarehouseListPageViewModel.EditModal)}.{nameof(WarehouseFormViewModel.Id)}", "Depo bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Depolar");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdateWarehouseAsync(warehouseId, new UpdateWarehouseRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            IsActive = model.IsActive
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Depo guncellendi.";
        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteWarehouseAsync(id, cancellationToken);
        TempData["Success"] = "Depo silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<WarehouseListPageViewModel> BuildIndexPageViewModelAsync(
        bool includeInactive,
        CancellationToken cancellationToken,
        WarehouseFormViewModel? createModal = null,
        WarehouseFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false)
    {
        var warehouses = await _service.GetWarehousesAsync(includeInactive, cancellationToken);

        return new WarehouseListPageViewModel
        {
            IncludeInactive = includeInactive,
            Warehouses = warehouses,
            CreateModal = createModal ?? new WarehouseFormViewModel(),
            EditModal = editModal ?? new WarehouseFormViewModel(),
            OpenCreateModal = openCreateModal,
            OpenEditModal = openEditModal
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
