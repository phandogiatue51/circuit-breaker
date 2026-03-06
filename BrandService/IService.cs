using DTOs;

namespace BrandService
{
    public interface IService
    {
        Task<IEnumerable<BrandDto>> GetAllAsync();

        Task<BrandDto?> GetByIdAsync(int id);

        Task<BrandDto> CreateAsync(CreateBrandDto dto);

        Task<BrandDto?> UpdateAsync(int id, UpdateBrandDto dto);
       
        Task<bool> DeleteAsync(int id);
    }
}
