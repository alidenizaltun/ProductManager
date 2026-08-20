using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.WebUI.Models.ProductOperations;

namespace ProductManagement.WebUI.Controllers;

public sealed class PriceListsController : Controller
{
    private readonly IProductOperationsService _service;

    public PriceListsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool includeInactive, Guid? editId, bool openCreateModal, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Fiyat Listeleri");
        var viewModel = await BuildIndexPageViewModelAsync(
            includeInactive,
            cancellationToken,
            openCreateModal: openCreateModal,
            editId: editId,
            openEditModal: editId.HasValue);

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Fiyat Listesi");
        ViewData["Title"] = "Yeni Fiyat Listesi";
        return View("Edit", new PriceListFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PriceListFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Fiyat Listesi");
            ViewData["Title"] = "Yeni Fiyat Listesi";
            return View("Edit", model);
        }

        var created = await _service.CreatePriceListAsync(MapToCreateRequest(model), cancellationToken);
        TempData["Success"] = "Fiyat listesi olusturuldu.";

        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal([Bind(Prefix = nameof(PriceListListPageViewModel.CreateModal))] PriceListFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Fiyat Listeleri");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreatePriceListAsync(MapToCreateRequest(model), cancellationToken);
        TempData["Success"] = "Fiyat listesi olusturuldu.";

        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var priceList = await _service.GetPriceListByIdAsync(id, cancellationToken);
        if (priceList is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Fiyat Listesi Duzenle");
        ViewData["Title"] = "Fiyat Listesi Duzenle";

        return View(new PriceListFormViewModel
        {
            Id = priceList.Id,
            Code = priceList.Code,
            Name = priceList.Name,
            Description = priceList.Description,
            CurrencyCode = priceList.CurrencyCode,
            IsActive = priceList.IsActive,
            ValidFrom = priceList.ValidFrom,
            ValidTo = priceList.ValidTo,
            SalesChannel = priceList.SalesChannel,
            CustomerGroupCode = priceList.CustomerGroupCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PriceListFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Fiyat Listesi Duzenle");
            ViewData["Title"] = "Fiyat Listesi Duzenle";
            return View(model);
        }

        var updated = await _service.UpdatePriceListAsync(id, MapToUpdateRequest(model), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Fiyat listesi guncellendi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal([Bind(Prefix = nameof(PriceListListPageViewModel.EditModal))] PriceListFormViewModel model, bool includeInactive, CancellationToken cancellationToken)
    {
        Guid priceListId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            priceListId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(PriceListListPageViewModel.EditModal)}.{nameof(PriceListFormViewModel.Id)}", "Fiyat listesi bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Fiyat Listeleri");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                includeInactive,
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdatePriceListAsync(priceListId, MapToUpdateRequest(model), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Fiyat listesi guncellendi.";
        return RedirectToAction(nameof(Index), new { includeInactive });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var viewModel = await BuildDetailsViewModelAsync(id, null, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Fiyat Listesi Detayi");
        ViewData["Title"] = "Fiyat Listesi Detayi";
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(Guid id, [Bind(Prefix = nameof(PriceListDetailsPageViewModel.NewItem))] PriceListItemFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.ProductId == Guid.Empty)
        {
            ModelState.AddModelError($"{nameof(PriceListDetailsPageViewModel.NewItem)}.{nameof(PriceListItemFormViewModel.ProductId)}", "Urun secimi zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            var invalidViewModel = await BuildDetailsViewModelAsync(id, model, cancellationToken, openNewItemModal: true);
            if (invalidViewModel is null)
            {
                return NotFound();
            }

            SetBreadcrumb("Fiyat Listesi Detayi");
            ViewData["Title"] = "Fiyat Listesi Detayi";
            return View("Details", invalidViewModel);
        }

        await _service.CreatePriceListItemAsync(new CreateProductPriceListItemRequestDto
        {
            ProductPriceListId = id,
            ProductId = model.ProductId,
            ProductVariantId = model.ProductVariantId,
            Amount = model.Amount,
            CompareAtAmount = model.CompareAtAmount,
            MinQuantity = model.MinQuantity,
            MaxQuantity = model.MaxQuantity
        }, cancellationToken);

        TempData["Success"] = "Fiyat listesi kalemi eklendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        await _service.DeletePriceListItemAsync(itemId, cancellationToken);
        TempData["Success"] = "Fiyat listesi kalemi silindi.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeletePriceListAsync(id, cancellationToken);
        TempData["Success"] = "Fiyat listesi silindi.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<PriceListDetailsPageViewModel?> BuildDetailsViewModelAsync(
        Guid id,
        PriceListItemFormViewModel? form,
        CancellationToken cancellationToken,
        bool openNewItemModal = false)
    {
        var priceListTask = _service.GetPriceListByIdAsync(id, cancellationToken);
        var itemsTask = _service.GetPriceListItemsAsync(id, cancellationToken);
        var productsTask = _service.GetProductsAsync(new ProductFilterDto { IsActive = true, Take = 200, IncludeLargeFields = false }, cancellationToken);

        await Task.WhenAll(priceListTask, itemsTask, productsTask);

        var priceList = await priceListTask;
        if (priceList is null)
        {
            return null;
        }

        return new PriceListDetailsPageViewModel
        {
            PriceList = priceList,
            Items = await itemsTask,
            Products = await productsTask,
            NewItem = form ?? new PriceListItemFormViewModel(),
            OpenNewItemModal = openNewItemModal
        };
    }

    private async Task<PriceListListPageViewModel> BuildIndexPageViewModelAsync(
        bool includeInactive,
        CancellationToken cancellationToken,
        PriceListFormViewModel? createModal = null,
        PriceListFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false,
        Guid? editId = null)
    {
        var priceLists = await _service.GetPriceListsAsync(includeInactive, cancellationToken);

        PriceListFormViewModel resolvedEditModal = editModal ?? new PriceListFormViewModel();
        var shouldOpenEditModal = openEditModal;

        if (editModal is null && editId.HasValue)
        {
            var editTarget = priceLists.FirstOrDefault(x => x.Id == editId.Value)
                             ?? await _service.GetPriceListByIdAsync(editId.Value, cancellationToken);

            if (editTarget is not null)
            {
                resolvedEditModal = MapToForm(editTarget);
            }
            else
            {
                shouldOpenEditModal = false;
            }
        }

        return new PriceListListPageViewModel
        {
            IncludeInactive = includeInactive,
            PriceLists = priceLists,
            CreateModal = createModal ?? new PriceListFormViewModel(),
            EditModal = resolvedEditModal,
            OpenCreateModal = openCreateModal,
            OpenEditModal = shouldOpenEditModal
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }

    private static CreateProductPriceListRequestDto MapToCreateRequest(PriceListFormViewModel model)
    {
        return new CreateProductPriceListRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant(),
            IsActive = model.IsActive,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            SalesChannel = model.SalesChannel,
            CustomerGroupCode = model.CustomerGroupCode
        };
    }

    private static UpdateProductPriceListRequestDto MapToUpdateRequest(PriceListFormViewModel model)
    {
        return new UpdateProductPriceListRequestDto
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description,
            CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant(),
            IsActive = model.IsActive,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            SalesChannel = model.SalesChannel,
            CustomerGroupCode = model.CustomerGroupCode
        };
    }

    private static PriceListFormViewModel MapToForm(ProductPriceListDto priceList)
    {
        return new PriceListFormViewModel
        {
            Id = priceList.Id,
            Code = priceList.Code,
            Name = priceList.Name,
            Description = priceList.Description,
            CurrencyCode = priceList.CurrencyCode,
            IsActive = priceList.IsActive,
            ValidFrom = priceList.ValidFrom,
            ValidTo = priceList.ValidTo,
            SalesChannel = priceList.SalesChannel,
            CustomerGroupCode = priceList.CustomerGroupCode
        };
    }
}
