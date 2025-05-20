using MicroservicesVisualizer.Models.Product;
using MicroservicesVisualizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesVisualizer.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IInventoryService inventoryService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // Product listing
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return View(Enumerable.Empty<ProductDto>());
            }
        }

        // Product details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null || product.Id == 0)
                {
                    return NotFound();
                }

                // Get inventory information for this product
                var inventories = await _inventoryService.GetInventoryByProductIdAsync(id);
                ViewBag.Inventories = inventories;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Category listing
        public async Task<IActionResult> Categories()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return View(Enumerable.Empty<CategoryDto>());
            }
        }

        // Category details
        public async Task<IActionResult> CategoryDetails(int id)
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(id);

                if (category == null || category.Id == 0)
                {
                    return NotFound();
                }

                // Get products in this category
                var products = await _productService.GetProductsByCategoryIdAsync(id);
                ViewBag.Products = products;

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category details for ID: {Id}", id);
                return RedirectToAction(nameof(Categories));
            }
        }

        // Search products
        public async Task<IActionResult> Search(string? keyword, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(keyword, minPrice, maxPrice);
                ViewBag.Keyword = keyword;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;

                return View("Index", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return RedirectToAction(nameof(Index));
            }
        }

        // Create product form
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                return View(new CreateProductDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product form");
                return RedirectToAction(nameof(Index));
            }
        }

        // Create product submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var product = await _productService.CreateProductAsync(model);
                    return RedirectToAction(nameof(Details), new { id = product.Id });
                }

                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");
                return View(model);
            }
        }

        // Edit product form
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null || product.Id == 0)
                {
                    return NotFound();
                }

                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                var model = new UpdateProductDto
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit product form for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Edit product submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _productService.UpdateProductAsync(id, model);
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {Id}", id);

                // Get categories for dropdown
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;

                ModelState.AddModelError("", "An error occurred while updating the product. Please try again.");
                return View(model);
            }
        }

        // Delete product confirmation
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null || product.Id == 0)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete product confirmation for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Delete product submit
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
