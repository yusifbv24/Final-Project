using MicroservicesVisualizer.Models.Product;

namespace MicroservicesVisualizer.Services.Interfaces
{
    public interface IProductService
    {
        // Product methods
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string? keyword, decimal? minPrice, decimal? maxPrice);

        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);

        // Category methods
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDto>> GetCategoriesWithProductsAsync();

        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
