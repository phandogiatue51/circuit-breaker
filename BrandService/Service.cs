using DTOs;

namespace BrandService
{
    public class Service : IService
    {
        private readonly Repository _repository;

        public Service(Repository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            var brands = await _repository.GetAllAsync();
            return brands.Select(MapToDto);
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _repository.GetByIdAsync(id);
            return brand != null ? MapToDto(brand) : null;
        }

        public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
        {
            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description,
            };

            await _repository.CreateAsync(brand);

            return MapToDto(brand);
        }

        public async Task<BrandDto?> UpdateAsync(int id, UpdateBrandDto dto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) return null;

            if (!string.IsNullOrEmpty(dto.Name))
                brand.Name = dto.Name;

            if (dto.Description != null)
                brand.Description = dto.Description;

            if (dto.IsActive.HasValue)
                brand.IsActive = dto.IsActive.Value;

            await _repository.UpdateAsync(brand);

            return MapToDto(brand);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        private BrandDto MapToDto(Brand brand)
        {
            return new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                IsActive = brand.IsActive
            };
        }
    }
}