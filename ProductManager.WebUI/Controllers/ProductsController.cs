using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.WebUI.Models.Products;

namespace ProductManager.WebUI.Controllers
{
    public sealed class ProductsController : Controller
    {
        private readonly IProductOperationsService _productOperationsService;

        private static readonly IReadOnlyDictionary<int, string> ProductKinds = new Dictionary<int, string>
        {
            [1] = "Fiziksel",
            [2] = "Yazılım",
            [3] = "Hizmet",
            [4] = "Abonelik",
            [5] = "Paket",
            [6] = "Dijital Varlık",
            [99] = "Diğer"
        };

        private static readonly IReadOnlyDictionary<int, string> ProductStatuses = new Dictionary<int, string>
        {
            [1] = "Draft",
            [2] = "Active",
            [3] = "Passive",
            [4] = "Archived"
        };

        private static readonly IReadOnlyList<string> CurrencyCodes =
        [
            "TRY",
            "USD",
            "EUR",
            "GBP"
        ];

        public ProductsController(IProductOperationsService productOperationsService)
        {
            _productOperationsService = productOperationsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProductIndexViewModel query, CancellationToken cancellationToken)
        {
            var take = NormalizeTake(query.Take);

            var products = await _productOperationsService.GetProductsAsync(
                new ProductFilterDto
                {
                    Search = query.Search,
                    Kind = query.Kind,
                    Status = query.Status,
                    IsActive = query.IsActive,
                    Take = take,
                    IncludeLargeFields = false
                },
                cancellationToken);

            var viewModel = new ProductIndexViewModel
            {
                Search = query.Search,
                Kind = query.Kind,
                Status = query.Status,
                IsActive = query.IsActive,
                Take = take,
                Products = products,
                KindLabels = ProductKinds,
                StatusLabels = ProductStatuses,
                KindOptions = BuildSelectOptions(ProductKinds, query.Kind, "Tüm Türler"),
                StatusOptions = BuildSelectOptions(ProductStatuses, query.Status, "Tüm Durumlar"),
                ActivityOptions = BuildActivityOptions(query.IsActive)
            };

            SetPageMetadata("Ürünler");
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = BuildProductForm(new ProductFormViewModel());
            SetPageMetadata("Yeni Ürün");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                SetPageMetadata("Yeni Ürün");
                return View(BuildProductForm(model));
            }

            try
            {
                await _productOperationsService.CreateProductAsync(MapToCreateRequest(model), cancellationToken);
                TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ürün oluşturulurken beklenmeyen bir hata oluştu.");
                SetPageMetadata("Yeni Ürün");
                return View(BuildProductForm(model));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Geçersiz ürün kimliği.";
                return RedirectToAction(nameof(Index));
            }

            var product = await _productOperationsService.GetProductByIdAsync(id, cancellationToken);
            if (product is null)
            {
                TempData["Error"] = "Güncellenecek ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = BuildProductForm(MapToForm(product));
            SetPageMetadata("Ürün Düzenle");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormViewModel model, CancellationToken cancellationToken)
        {
            if (model.Id is null || model.Id == Guid.Empty)
            {
                TempData["Error"] = "Geçersiz ürün kimliği.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                SetPageMetadata("Ürün Düzenle");
                return View(BuildProductForm(model));
            }

            try
            {
                var updated = await _productOperationsService.UpdateProductAsync(model.Id.Value, MapToUpdateRequest(model), cancellationToken);

                if (!updated)
                {
                    TempData["Error"] = "Ürün güncellenemedi veya bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Ürün güncellenirken beklenmeyen bir hata oluştu.");
                SetPageMetadata("Ürün Düzenle");
                return View(BuildProductForm(model));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Geçersiz ürün kimliği.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var deleted = await _productOperationsService.DeleteProductAsync(id, cancellationToken);
                TempData[deleted ? "Success" : "Error"] = deleted
                    ? "Ürün başarıyla silindi."
                    : "Ürün silinemedi veya bulunamadı.";
            }
            catch
            {
                TempData["Error"] = "Ürün silinirken beklenmeyen bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        private ProductFormViewModel BuildProductForm(ProductFormViewModel model)
        {
            model.KindOptions = BuildSelectOptions(ProductKinds, model.Kind);
            model.StatusOptions = BuildSelectOptions(ProductStatuses, model.Status);
            model.CurrencyOptions = BuildCurrencyOptions(model.DefaultCurrencyCode);
            model.DefaultCurrencyCode = NormalizeCurrency(model.DefaultCurrencyCode);
            return model;
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
                ProductCode = NormalizeRequired(model.ProductCode),
                Name = NormalizeRequired(model.Name),
                ShortDescription = NormalizeOptional(model.ShortDescription),
                Description = NormalizeOptional(model.Description),
                Kind = model.Kind,
                Status = model.Status,
                Brand = NormalizeOptional(model.Brand),
                Manufacturer = NormalizeOptional(model.Manufacturer),
                Barcode = NormalizeOptional(model.Barcode),
                IsActive = model.IsActive,
                IsSellable = model.IsSellable,
                IsPurchasable = model.IsPurchasable,
                TrackInventory = model.TrackInventory,
                DefaultCurrencyCode = NormalizeCurrency(model.DefaultCurrencyCode),
                UnitOfMeasure = NormalizeOptional(model.UnitOfMeasure),
                TaxRate = model.TaxRate,
                TaxCode = NormalizeOptional(model.TaxCode),
                Tags = NormalizeOptional(model.Tags),
                MetadataJson = null
            };
        }

        private static UpdateProductRequestDto MapToUpdateRequest(ProductFormViewModel model)
        {
            return new UpdateProductRequestDto
            {
                ProductCode = NormalizeRequired(model.ProductCode),
                Name = NormalizeRequired(model.Name),
                ShortDescription = NormalizeOptional(model.ShortDescription),
                Description = NormalizeOptional(model.Description),
                Kind = model.Kind,
                Status = model.Status,
                Brand = NormalizeOptional(model.Brand),
                Manufacturer = NormalizeOptional(model.Manufacturer),
                Barcode = NormalizeOptional(model.Barcode),
                IsActive = model.IsActive,
                IsSellable = model.IsSellable,
                IsPurchasable = model.IsPurchasable,
                TrackInventory = model.TrackInventory,
                DefaultCurrencyCode = NormalizeCurrency(model.DefaultCurrencyCode),
                UnitOfMeasure = NormalizeOptional(model.UnitOfMeasure),
                TaxRate = model.TaxRate,
                TaxCode = NormalizeOptional(model.TaxCode),
                Tags = NormalizeOptional(model.Tags),
                MetadataJson = null
            };
        }

        private void SetPageMetadata(string breadcrumb)
        {
            ViewData["Title"] = breadcrumb;
            ViewData["Breadcrumb"] = breadcrumb;
        }

        private static IReadOnlyList<SelectListItem> BuildSelectOptions(
            IReadOnlyDictionary<int, string> source,
            int? selectedValue,
            string? emptyLabel = null)
        {
            var items = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(emptyLabel))
            {
                items.Add(new SelectListItem(emptyLabel, string.Empty, !selectedValue.HasValue));
            }

            foreach (var item in source)
            {
                items.Add(new SelectListItem(item.Value, item.Key.ToString(), selectedValue == item.Key));
            }

            return items;
        }

        private static IReadOnlyList<SelectListItem> BuildCurrencyOptions(string? selectedCurrency)
        {
            var selected = NormalizeCurrency(selectedCurrency);
            var items = new List<SelectListItem>(CurrencyCodes.Count);

            foreach (var code in CurrencyCodes)
            {
                items.Add(new SelectListItem(code, code, selected.Equals(code, StringComparison.OrdinalIgnoreCase)));
            }

            return items;
        }

        private static IReadOnlyList<SelectListItem> BuildActivityOptions(bool? selected)
        {
            return
            [
                new SelectListItem("Tüm Durumlar", string.Empty, !selected.HasValue),
                new SelectListItem("Sadece Aktif", bool.TrueString.ToLowerInvariant(), selected is true),
                new SelectListItem("Sadece Pasif", bool.FalseString.ToLowerInvariant(), selected is false)
            ];
        }

        private static string NormalizeRequired(string value) => value.Trim();

        private static string? NormalizeOptional(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string NormalizeCurrency(string? value)
            => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();

        private static int NormalizeTake(int take)
            => take <= 0 ? 50 : Math.Min(take, 200);
    }
}
