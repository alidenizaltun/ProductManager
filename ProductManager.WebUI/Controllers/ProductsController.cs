using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.WebUI.Models.ProductOperations;

namespace ProductManager.WebUI.Controllers;

public sealed class ProductsController : Controller
{
    private readonly IProductOperationsService _service;

    public ProductsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductFilterInput filter, Guid? editId, bool openCreateModal, CancellationToken cancellationToken)
    {
        SetBreadcrumb("Urunler");
        var viewModel = await BuildIndexPageViewModelAsync(
            filter,
            cancellationToken,
            openCreateModal: openCreateModal,
            editId: editId,
            openEditModal: editId.HasValue);

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetBreadcrumb("Yeni Urun");
        ViewData["Title"] = "Yeni Urun";
        return View("Edit", new ProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Yeni Urun");
            ViewData["Title"] = "Yeni Urun";
            return View("Edit", model);
        }

        var created = await _service.CreateProductAsync(MapToCreateRequest(model), cancellationToken);
        TempData["Success"] = "Urun basariyla olusturuldu.";

        return RedirectToAction(nameof(Edit), new { id = created.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromModal(
        [Bind(Prefix = nameof(ProductListPageViewModel.CreateModal))] ProductFormViewModel model,
        [Bind(Prefix = nameof(ProductListPageViewModel.Filter))] ProductFilterInput filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Urunler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                filter,
                cancellationToken,
                createModal: model,
                openCreateModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        await _service.CreateProductAsync(MapToCreateRequest(model), cancellationToken);
        TempData["Success"] = "Urun basariyla olusturuldu.";

        return RedirectToAction(nameof(Index), BuildFilterRouteValues(filter));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var product = await _service.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        SetBreadcrumb("Urun Duzenle");
        ViewData["Title"] = "Urun Duzenle";

        return View(MapToForm(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Urun Duzenle");
            ViewData["Title"] = "Urun Duzenle";
            return View(model);
        }

        var updated = await _service.UpdateProductAsync(id, MapToUpdateRequest(model), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Urun bilgileri guncellendi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFromModal(
        [Bind(Prefix = nameof(ProductListPageViewModel.EditModal))] ProductFormViewModel model,
        [Bind(Prefix = nameof(ProductListPageViewModel.Filter))] ProductFilterInput filter,
        CancellationToken cancellationToken)
    {
        Guid productId = Guid.Empty;
        if (model.Id is Guid parsedId)
        {
            productId = parsedId;
        }
        else
        {
            ModelState.AddModelError($"{nameof(ProductListPageViewModel.EditModal)}.{nameof(ProductFormViewModel.Id)}", "Urun bilgisi bulunamadi.");
        }

        if (!ModelState.IsValid)
        {
            SetBreadcrumb("Urunler");
            var invalidViewModel = await BuildIndexPageViewModelAsync(
                filter,
                cancellationToken,
                editModal: model,
                openEditModal: true);

            return View(nameof(Index), invalidViewModel);
        }

        var updated = await _service.UpdateProductAsync(productId, MapToUpdateRequest(model), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        TempData["Success"] = "Urun bilgileri guncellendi.";
        return RedirectToAction(nameof(Index), BuildFilterRouteValues(filter));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var product = await _service.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var categoryMapsTask = _service.GetProductCategoryMapsAsync(id, cancellationToken);
        var categoriesTask = _service.GetCategoriesAsync(cancellationToken);
        var attributeDefinitionsTask = _service.GetAttributeDefinitionsAsync(cancellationToken);
        var attributeValuesTask = _service.GetProductAttributeValuesAsync(id, cancellationToken);
        var mediaTask = _service.GetProductMediaAsync(id, cancellationToken);
        var bundleItemsTask = _service.GetBundleItemsAsync(id, cancellationToken);
        var variantsTask = _service.GetProductVariantsAsync(id, cancellationToken);
        var pricesTask = _service.GetProductPricesAsync(id, cancellationToken);
        var inventoriesTask = _service.GetProductInventoriesAsync(new ProductInventoryFilterDto { ProductId = id, Take = 200 }, cancellationToken);
        var supplierMapsTask = _service.GetProductSupplierMapsAsync(id, cancellationToken);
        var suppliersTask = _service.GetSuppliersAsync(true, cancellationToken);
        var physicalProfileTask = _service.GetPhysicalProfileAsync(id, cancellationToken);
        var softwareProfileTask = _service.GetSoftwareProfileAsync(id, cancellationToken);
        var serviceProfileTask = _service.GetServiceProfileAsync(id, cancellationToken);
        var subscriptionProfileTask = _service.GetSubscriptionProfileAsync(id, cancellationToken);

        await Task.WhenAll(
            categoryMapsTask,
            categoriesTask,
            attributeDefinitionsTask,
            attributeValuesTask,
            mediaTask,
            bundleItemsTask,
            variantsTask,
            pricesTask,
            inventoriesTask,
            supplierMapsTask,
            suppliersTask,
            physicalProfileTask,
            softwareProfileTask,
            serviceProfileTask,
            subscriptionProfileTask);

        var viewModel = new ProductDetailsViewModel
        {
            Product = product,
            CategoryMaps = await categoryMapsTask,
            Categories = await categoriesTask,
            AttributeDefinitions = await attributeDefinitionsTask,
            AttributeValues = await attributeValuesTask,
            Media = await mediaTask,
            BundleItems = await bundleItemsTask,
            Variants = await variantsTask,
            Prices = await pricesTask,
            Inventories = await inventoriesTask,
            SupplierMaps = await supplierMapsTask,
            Suppliers = await suppliersTask,
            PhysicalProfile = await physicalProfileTask,
            SoftwareProfile = await softwareProfileTask,
            ServiceProfile = await serviceProfileTask,
            SubscriptionProfile = await subscriptionProfileTask
        };

        SetBreadcrumb("Urun Detayi");
        ViewData["Title"] = "Urun Detayi";

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteProductAsync(id, cancellationToken);
        TempData["Success"] = "Urun silindi.";

        return RedirectToAction(nameof(Index));
    }

    private void SetBreadcrumb(string breadcrumb)
    {
        ViewData["Breadcrumb"] = breadcrumb;
    }

    private async Task<ProductListPageViewModel> BuildIndexPageViewModelAsync(
        ProductFilterInput filter,
        CancellationToken cancellationToken,
        ProductFormViewModel? createModal = null,
        ProductFormViewModel? editModal = null,
        bool openCreateModal = false,
        bool openEditModal = false,
        Guid? editId = null)
    {
        var normalizedFilter = new ProductFilterInput
        {
            Search = filter.Search,
            Kind = filter.Kind,
            Status = filter.Status,
            IsActive = filter.IsActive,
            Take = NormalizeTake(filter.Take)
        };

        var products = await _service.GetProductsAsync(
            new ProductFilterDto
            {
                Search = normalizedFilter.Search,
                Kind = normalizedFilter.Kind,
                Status = normalizedFilter.Status,
                IsActive = normalizedFilter.IsActive,
                Take = normalizedFilter.Take
            },
            cancellationToken);

        ProductFormViewModel resolvedEditModal = editModal ?? new ProductFormViewModel();
        var shouldOpenEditModal = openEditModal;

        if (editModal is null && editId.HasValue)
        {
            var editTarget = products.FirstOrDefault(x => x.Id == editId.Value)
                             ?? await _service.GetProductByIdAsync(editId.Value, cancellationToken);

            if (editTarget is not null)
            {
                resolvedEditModal = MapToForm(editTarget);
            }
            else
            {
                shouldOpenEditModal = false;
            }
        }

        return new ProductListPageViewModel
        {
            Filter = normalizedFilter,
            Products = products,
            CreateModal = createModal ?? new ProductFormViewModel(),
            EditModal = resolvedEditModal,
            OpenCreateModal = openCreateModal,
            OpenEditModal = shouldOpenEditModal
        };
    }

    private static object BuildFilterRouteValues(ProductFilterInput filter)
    {
        return new
        {
            filter.Search,
            filter.Kind,
            filter.Status,
            filter.IsActive,
            Take = NormalizeTake(filter.Take)
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

    private static ProductFormViewModel MapToForm(ProductDto product)
    {
        return new ProductFormViewModel
        {
            Id = product.Id,
            ProductCode = product.ProductCode,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Kind = product.Kind,
            Status = product.Status,
            Brand = product.Brand,
            Manufacturer = product.Manufacturer,
            Barcode = product.Barcode,
            IsActive = product.IsActive,
            IsSellable = product.IsSellable,
            IsPurchasable = product.IsPurchasable,
            TrackInventory = product.TrackInventory,
            DefaultCurrencyCode = product.DefaultCurrencyCode,
            UnitOfMeasure = product.UnitOfMeasure,
            TaxRate = product.TaxRate,
            TaxCode = product.TaxCode,
            Tags = product.Tags,
            MetadataJson = product.MetadataJson
        };
    }

    private static CreateProductRequestDto MapToCreateRequest(ProductFormViewModel model)
    {
        return new CreateProductRequestDto
        {
            ProductCode = model.ProductCode.Trim(),
            Name = model.Name.Trim(),
            ShortDescription = model.ShortDescription,
            Description = model.Description,
            Kind = model.Kind,
            Status = model.Status,
            Brand = model.Brand,
            Manufacturer = model.Manufacturer,
            Barcode = model.Barcode,
            IsActive = model.IsActive,
            IsSellable = model.IsSellable,
            IsPurchasable = model.IsPurchasable,
            TrackInventory = model.TrackInventory,
            DefaultCurrencyCode = model.DefaultCurrencyCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = model.UnitOfMeasure,
            TaxRate = model.TaxRate,
            TaxCode = model.TaxCode,
            Tags = model.Tags,
            MetadataJson = model.MetadataJson
        };
    }

    private static UpdateProductRequestDto MapToUpdateRequest(ProductFormViewModel model)
    {
        return new UpdateProductRequestDto
        {
            ProductCode = model.ProductCode.Trim(),
            Name = model.Name.Trim(),
            ShortDescription = model.ShortDescription,
            Description = model.Description,
            Kind = model.Kind,
            Status = model.Status,
            Brand = model.Brand,
            Manufacturer = model.Manufacturer,
            Barcode = model.Barcode,
            IsActive = model.IsActive,
            IsSellable = model.IsSellable,
            IsPurchasable = model.IsPurchasable,
            TrackInventory = model.TrackInventory,
            DefaultCurrencyCode = model.DefaultCurrencyCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = model.UnitOfMeasure,
            TaxRate = model.TaxRate,
            TaxCode = model.TaxCode,
            Tags = model.Tags,
            MetadataJson = model.MetadataJson
        };
    }
}
