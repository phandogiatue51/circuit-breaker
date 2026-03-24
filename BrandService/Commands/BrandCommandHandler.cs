using DTOs;
using BrandService.Mappers;

namespace BrandService.Commands
{
    public class BrandCommandHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<BrandCommandHandler> _logger;

        public BrandCommandHandler(
            Repository repository,
            ILogger<BrandCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// COMMAND: Tạo thương hiệu mới
        /// </summary>
        public async Task<BrandDto> Handle(CreateBrandCommand command)
        {
            _logger.LogInformation("Handling CreateBrandCommand: {Name}", command.Name);

            // Create brand
            var brand = new Brand
            {
                Name = command.Name,
                Description = command.Description
            };

            await _repository.CreateAsync(brand);
            _logger.LogInformation("Brand created: {Id} - {Name}", brand.Id, brand.Name);

            return BrandMapper.ToDto(brand);
        }

        /// <summary>
        /// COMMAND: Cập nhật thương hiệu
        /// </summary>
        public async Task<BrandDto?> Handle(UpdateBrandCommand command, int id)
        {
            _logger.LogInformation("Handling UpdateBrandCommand for id: {Id}", id);

            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
            {
                return null;
            }

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(command.Name))
                brand.Name = command.Name;

            if (!string.IsNullOrWhiteSpace(command.Description))
                brand.Description = command.Description;

            brand.UpdatedAt = DateTime.UtcNow.AddHours(7);
            await _repository.UpdateAsync(brand);
            _logger.LogInformation("Brand updated: {Id}", brand.Id);

            return BrandMapper.ToDto(brand);
        }

        /// <summary>
        /// COMMAND: Xóa thương hiệu
        /// </summary>
        public async Task<bool> Handle(DeleteBrandCommand command)
        {
            _logger.LogInformation("Handling DeleteBrandCommand for id: {Id}", command.Id);

            var exists = await _repository.ExistsAsync(command.Id);
            if (!exists) return false;

            await _repository.DeleteAsync(command.Id);
            _logger.LogInformation("Brand deleted: {Id}", command.Id);

            return true;
        }
    }
}