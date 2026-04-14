using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.WebUI.Models.ProductOperations;

namespace ProductManager.WebUI.Controllers;

public sealed class SuppliersController : Controller
{
    private readonly IProductOperationsService _service;

    public SuppliersController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool includeInactive, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Tedarikciler");
        var viewModel = await BuildIndexPageViewModelAsync(includeInactive, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Tedarikci");
        ViewData["Title"] = "Yeni Tedarikci";
        return View("Edit", new SupplierFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Tedarikci");
            ViewData["Title"] = "Yeni Tedarikci";
            return View("Edit", model);
        }

        await _service.CreateSupplierAsync(new CreateProductSupplierRequestDto
        {
            SupplierCode = model.SupplierCode.Trim(),
            Name = model.Name.Trim(),
            TaxNumber = model.TaxNumber,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            IsActive = model.IsActive
        }, cancellationToken);

        TempData["Success"] = "Tedarikci olusturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal([Bind(Prefix = nameof(SupplierListPageViewModel.CreateModal))] SupplierFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Tedarikciler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreateSupplierAsync(new CreateProductSupplierRequestDto
        {
            SupplierCode = model.SupplierCode.Trim(),
            Name = model.Name.Trim(),
            TaxNumber = model.TaxNumber,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            IsActive = model.IsActive
        }, cancellationToken);

        TempData["Success"] = "Tedarikci olusturuldu.";
        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _service.GetSupplierByIdAsync(id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Tedarikci Duzenle");
        ViewData["Title"] = "Tedarikci Duzenle";

        return View(new SupplierFormViewModel
        {
            Id = supplier.Id,
            SupplierCode = supplier.SupplierCode,
            Name = supplier.Name,
            TaxNumber = supplier.TaxNumber,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SupplierFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Tedarikci Duzenle");
            ViewData["Title"] = "Tedarikci Duzenle";
            return View(model);
        }

        var updated = await _service.UpdateSupplierAsync(id, new UpdateProductSupplierRequestDto
        {
            SupplierCode = model.SupplierCode.Trim(),
            Name = model.Name.Trim(),
            TaxNumber = model.TaxNumber,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            IsActive = model.IsActive
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Tedarikci guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal([Bind(Prefix = nameof(SupplierListPageViewModel.EditModal))] SupplierFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        Guid supplierId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            supplierId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(SupplierListPageViewModel.EditModal)}.{nameof(SupplierFormViewModel.Id)}", "Tedarikci bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Tedarikciler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdateSupplierAsync(supplierId, new UpdateProductSupplierRequestDto
        {
            SupplierCode = model.SupplierCode.Trim(),
            Name = model.Name.Trim(),
            TaxNumber = model.TaxNumber,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            IsActive = model.IsActive
        }, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Tedarikci guncellendi.";
        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteSupplierAsync(id, cancellationToken);
        TempData["Success"] = "Tedarikci silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SupplierListPageViewModel> BuildIndexPageViewModelAsync(
        bool includeInactive,
        CancellationToken cancellationToken,
        SupplierFormViewModel? createModal = null,
        SupplierFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false)
    {
        var suppliers = await _service.GetSuppliersAsync(includeInactive, cancellationToken);

        return new SupplierListPageViewModel
        {
            IncludeInactive = includeInactive,
            Suppliers = suppliers,
            CreateModal = createModal ?? new SupplierFormViewModel(),
            EditModal = editModal ?? new SupplierFormViewModel(),
            OpenCreateModal = openCreateModal,
            OpenEditModal = openEditModal
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
