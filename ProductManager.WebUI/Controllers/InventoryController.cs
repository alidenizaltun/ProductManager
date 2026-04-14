using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.WebUI.Models.ProductOperations;

namespace ProductManager.WebUI.Controllers;

public sealed class InventoryController : Controller
{
    private readonly IProductOperationsService _service;

    public InventoryController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Transactions([FromQuery] InventoryTransactionFilterInput filter, bool openCreateModal, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Stok Hareketleri");

        var viewModel = await BuildTransactionsPageViewModelAsync(
            filter,
            cancellationToken,
            openCreateModal: openCreateModal);

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateTransaction()
    {
        return RedirectToAction(nameof(Transactions), new { openCreateModal = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(
        [Bind(Prefix = nameof(InventoryTransactionListPageViewModel.CreateModal))] InventoryTransactionFormViewModel model,
        [Bind(Prefix = nameof(InventoryTransactionListPageViewModel.Filter))] InventoryTransactionFilterInput filter,
        CancellationToken cancellationToken)
    {
        if (model.ProductId == Guid.Empty)
        {
            ModelState.AddModelError($"{nameof(InventoryTransactionListPageViewModel.CreateModal)}.{nameof(InventoryTransactionFormViewModel.ProductId)}", "Urun secimi zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Stok Hareketleri");
            var invalidViewModel = await BuildTransactionsPageViewModelAsync(
                filter,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Transactions), invalidViewModel);
        }

        await _service.CreateInventoryTransactionAsync(new CreateInventoryTransactionRequestDto
        {
            ProductId = model.ProductId,
            ProductVariantId = model.ProductVariantId,
            WarehouseId = model.WarehouseId,
            TransactionType = model.TransactionType,
            Quantity = model.Quantity,
            UnitCost = model.UnitCost,
            ReferenceType = model.ReferenceType,
            ReferenceNumber = model.ReferenceNumber,
            Note = model.Note,
            OccurredAt = model.OccurredAt
        }, cancellationToken);

        TempData["Success"] = "Stok hareketi olusturuldu.";
        return RedirectToAction(nameof(Transactions), BuildTransactionFilterRouteValues(filter));
    }

    [HttpGet]
    public async Task<IActionResult> Reservations([FromQuery] InventoryReservationFilterInput filter, bool openCreateModal, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Stok Rezervasyonlari");

        var viewModel = await BuildReservationsPageViewModelAsync(
            filter,
            cancellationToken,
            openCreateModal: openCreateModal);

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateReservation()
    {
        return RedirectToAction(nameof(Reservations), new { openCreateModal = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(
        [Bind(Prefix = nameof(InventoryReservationListPageViewModel.CreateModal))] InventoryReservationFormViewModel model,
        [Bind(Prefix = nameof(InventoryReservationListPageViewModel.Filter))] InventoryReservationFilterInput filter,
        CancellationToken cancellationToken)
    {
        if (model.ProductId == Guid.Empty)
        {
            ModelState.AddModelError($"{nameof(InventoryReservationListPageViewModel.CreateModal)}.{nameof(InventoryReservationFormViewModel.ProductId)}", "Urun secimi zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Stok Rezervasyonlari");
            var invalidViewModel = await BuildReservationsPageViewModelAsync(
                filter,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Reservations), invalidViewModel);
        }

        await _service.CreateInventoryReservationAsync(new CreateInventoryReservationRequestDto
        {
            ProductId = model.ProductId,
            ProductVariantId = model.ProductVariantId,
            WarehouseId = model.WarehouseId,
            Quantity = model.Quantity,
            ReservationCode = model.ReservationCode.Trim(),
            ReservedUntil = model.ReservedUntil,
            Status = model.Status,
            SourceType = model.SourceType,
            SourceId = model.SourceId
        }, cancellationToken);

        TempData["Success"] = "Rezervasyon olusturuldu.";
        return RedirectToAction(nameof(Reservations), BuildReservationFilterRouteValues(filter));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReservationStatus(
        [Bind(Prefix = nameof(InventoryReservationListPageViewModel.UpdateStatusModal))] InventoryReservationStatusFormViewModel model,
        [Bind(Prefix = nameof(InventoryReservationListPageViewModel.Filter))] InventoryReservationFilterInput filter,
        CancellationToken cancellationToken)
    {
        if (model.Id == Guid.Empty)
        {
            ModelState.AddModelError($"{nameof(InventoryReservationListPageViewModel.UpdateStatusModal)}.{nameof(InventoryReservationStatusFormViewModel.Id)}", "Rezervasyon secimi zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Stok Rezervasyonlari");
            var invalidViewModel = await BuildReservationsPageViewModelAsync(
                filter,
                cancellationToken,
                statusModal: model,
                openUpdateStatusModal: true);

            return View(nameof(Reservations), invalidViewModel);
        }

        await _service.UpdateInventoryReservationStatusAsync(model.Id, new UpdateInventoryReservationStatusRequestDto
        {
            Status = model.Status,
            ReservedUntil = model.ReservedUntil
        }, cancellationToken);

        TempData["Success"] = "Rezervasyon durumu guncellendi.";
        return RedirectToAction(nameof(Reservations), BuildReservationFilterRouteValues(filter));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReservation(Guid id, [Bind(Prefix = nameof(InventoryReservationListPageViewModel.Filter))] InventoryReservationFilterInput filter, CancellationToken cancellationToken)
    {
        await _service.DeleteInventoryReservationAsync(id, cancellationToken);
        TempData["Success"] = "Rezervasyon silindi.";
        return RedirectToAction(nameof(Reservations), BuildReservationFilterRouteValues(filter));
    }

    private async Task<InventoryTransactionListPageViewModel> BuildTransactionsPageViewModelAsync(
        InventoryTransactionFilterInput filter,
        CancellationToken cancellationToken,
        InventoryTransactionFormViewModel? createModal = null,
        bool openCreateModal = false)
    {
        var normalizedFilter = new InventoryTransactionFilterInput
        {
            ProductId = filter.ProductId,
            ProductVariantId = filter.ProductVariantId,
            WarehouseId = filter.WarehouseId,
            TransactionType = filter.TransactionType,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            Take = NormalizeTake(filter.Take)
        };

        var transactionsTask = _service.GetInventoryTransactionsAsync(
            new InventoryTransactionFilterDto
            {
                ProductId = normalizedFilter.ProductId,
                ProductVariantId = normalizedFilter.ProductVariantId,
                WarehouseId = normalizedFilter.WarehouseId,
                TransactionType = normalizedFilter.TransactionType,
                DateFrom = normalizedFilter.DateFrom,
                DateTo = normalizedFilter.DateTo,
                Take = normalizedFilter.Take
            },
            cancellationToken);

        var modalModelTask = BuildTransactionFormAsync(createModal ?? new InventoryTransactionFormViewModel(), cancellationToken);

        await Task.WhenAll(transactionsTask, modalModelTask);

        return new InventoryTransactionListPageViewModel
        {
            Filter = normalizedFilter,
            Transactions = await transactionsTask,
            CreateModal = await modalModelTask,
            OpenCreateModal = openCreateModal
        };
    }

    private async Task<InventoryReservationListPageViewModel> BuildReservationsPageViewModelAsync(
        InventoryReservationFilterInput filter,
        CancellationToken cancellationToken,
        InventoryReservationFormViewModel? createModal = null,
        InventoryReservationStatusFormViewModel? statusModal = null,
        bool openCreateModal = false,
        bool openUpdateStatusModal = false)
    {
        var normalizedFilter = new InventoryReservationFilterInput
        {
            ProductId = filter.ProductId,
            ProductVariantId = filter.ProductVariantId,
            WarehouseId = filter.WarehouseId,
            Status = filter.Status,
            ReservedUntilMin = filter.ReservedUntilMin,
            ReservedUntilMax = filter.ReservedUntilMax,
            Take = NormalizeTake(filter.Take)
        };

        var reservationsTask = _service.GetInventoryReservationsAsync(
            new InventoryReservationFilterDto
            {
                ProductId = normalizedFilter.ProductId,
                ProductVariantId = normalizedFilter.ProductVariantId,
                WarehouseId = normalizedFilter.WarehouseId,
                Status = normalizedFilter.Status,
                ReservedUntilMin = normalizedFilter.ReservedUntilMin,
                ReservedUntilMax = normalizedFilter.ReservedUntilMax,
                Take = normalizedFilter.Take
            },
            cancellationToken);

        var createModalModelTask = BuildReservationFormAsync(createModal ?? new InventoryReservationFormViewModel(), cancellationToken);

        await Task.WhenAll(reservationsTask, createModalModelTask);

        return new InventoryReservationListPageViewModel
        {
            Filter = normalizedFilter,
            Reservations = await reservationsTask,
            CreateModal = await createModalModelTask,
            UpdateStatusModal = statusModal ?? new InventoryReservationStatusFormViewModel(),
            OpenCreateModal = openCreateModal,
            OpenUpdateStatusModal = openUpdateStatusModal
        };
    }

    private async Task<InventoryTransactionFormViewModel> BuildTransactionFormAsync(InventoryTransactionFormViewModel model, CancellationToken cancellationToken)
    {
        var productsTask = _service.GetProductsAsync(new ProductFilterDto { IsActive = true, Take = 200 }, cancellationToken);
        var warehousesTask = _service.GetWarehousesAsync(true, cancellationToken);

        await Task.WhenAll(productsTask, warehousesTask);

        return new InventoryTransactionFormViewModel
        {
            ProductId = model.ProductId,
            ProductVariantId = model.ProductVariantId,
            WarehouseId = model.WarehouseId,
            TransactionType = model.TransactionType,
            Quantity = model.Quantity,
            UnitCost = model.UnitCost,
            ReferenceType = model.ReferenceType,
            ReferenceNumber = model.ReferenceNumber,
            Note = model.Note,
            OccurredAt = model.OccurredAt,
            Products = await productsTask,
            Warehouses = await warehousesTask
        };
    }

    private async Task<InventoryReservationFormViewModel> BuildReservationFormAsync(InventoryReservationFormViewModel model, CancellationToken cancellationToken)
    {
        var productsTask = _service.GetProductsAsync(new ProductFilterDto { IsActive = true, Take = 200 }, cancellationToken);
        var warehousesTask = _service.GetWarehousesAsync(true, cancellationToken);

        await Task.WhenAll(productsTask, warehousesTask);

        return new InventoryReservationFormViewModel
        {
            ProductId = model.ProductId,
            ProductVariantId = model.ProductVariantId,
            WarehouseId = model.WarehouseId,
            Quantity = model.Quantity,
            ReservationCode = model.ReservationCode,
            ReservedUntil = model.ReservedUntil,
            Status = model.Status,
            SourceType = model.SourceType,
            SourceId = model.SourceId,
            Products = await productsTask,
            Warehouses = await warehousesTask
        };
    }

    private static int NormalizeTake(int take)
    {
        if (take < 1)
        {
            return 100;
        }

        return take > 500 ? 500 : take;
    }

    private static object BuildTransactionFilterRouteValues(InventoryTransactionFilterInput filter)
    {
        return new
        {
            filter.ProductId,
            filter.ProductVariantId,
            filter.WarehouseId,
            filter.TransactionType,
            filter.DateFrom,
            filter.DateTo,
            Take = NormalizeTake(filter.Take)
        };
    }

    private static object BuildReservationFilterRouteValues(InventoryReservationFilterInput filter)
    {
        return new
        {
            filter.ProductId,
            filter.ProductVariantId,
            filter.WarehouseId,
            filter.Status,
            filter.ReservedUntilMin,
            filter.ReservedUntilMax,
            Take = NormalizeTake(filter.Take)
        };
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }
}
