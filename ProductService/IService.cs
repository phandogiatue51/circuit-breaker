using DTOs;

namespace ProductService
{
    public interface IService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetByBrandIdAsync(int brandId);
        Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}