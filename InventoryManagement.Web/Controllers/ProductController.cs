using InventoryManagement.Web.Models.Product;
using InventoryManagement.Web.Services.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ProductApiClient _productApiClient;
        private readonly CategoryApiClient _categoryApiClient;

        public ProductController(
            ILogger<ProductController> logger,
            ProductApiClient productApiClient,
            CategoryApiClient categoryApiClient)
        {
            _logger = logger;
            _productApiClient = productApiClient;
            _categoryApiClient = categoryApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productApiClient.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productApiClient.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryApiClient.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new ProductViewModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    SKU = model.SKU,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    IsActive = true
                };

                var createdProduct = await _productApiClient.CreateProductAsync(product);
                if (createdProduct != null)
                {
                    return RedirectToAction(nameof(Details), new { id = createdProduct.Id });
                }
            }

            var categories = await _categoryApiClient.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productApiClient.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryApiClient.GetAllCategoriesAsync();
            ViewBag.Categories = categories;

            var model = new EditProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProductViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var product = new ProductViewModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    SKU = model.SKU,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    IsActive = model.IsActive
                };

                var updatedProduct = await _productApiClient.UpdateProductAsync(id, product);
                if (updatedProduct != null)
                {
                    return RedirectToAction(nameof(Details), new { id = updatedProduct.Id });
                }
            }

            var categories = await _categoryApiClient.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productApiClient.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productApiClient.DeleteProductAsync(id);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Delete), new { id = id });
        }
    }

}
