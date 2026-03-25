using DTOs;
using BrandService.Mappers;
using Cloud;

namespace BrandService.Commands
{
    public class BrandCommandHandler
    {
        private readonly Repository _repository;
        private readonly EventStoreService _eventStore;
        private readonly ILogger<BrandCommandHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public BrandCommandHandler(
            Repository repository,
            EventStoreService eventStore,
            ILogger<BrandCommandHandler> logger, CloudinaryService cloudinaryService)
        {
            _repository = repository;
            _logger = logger;
            _eventStore = eventStore;  
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// COMMAND: Tạo thương hiệu mới
        /// </summary>
        public async Task<BrandDto> Handle(CreateBrandCommand command)
        {
            _logger.LogInformation("Handling CreateBrandCommand: {Name}", command.Name);

            // Upload logo to Cloudinary
            string? imageUrl = null;
            if (command.Image != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(command.Image);
                _logger.LogInformation("Logo uploaded: {LogoUrl}", imageUrl);
            }

            // Create brand
            var brand = new Brand
            {
                Name = command.Name,
                Description = command.Description,
                ImageUrl = imageUrl,
            };

            await _repository.CreateAsync(brand);
            _logger.LogInformation("Brand created: {Id} - {Name}", brand.Id, brand.Name);

            await _eventStore.SaveEventAsync(brand.Id, "BrandCreated", new
            {
                brand.Id,
                brand.Name,
                brand.Description,
                brand.ImageUrl
            });

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

            if (command.Image != null)
            {
                if (!string.IsNullOrEmpty(brand.ImageUrl))
                {
                    var deleted = await _cloudinaryService.DeleteImageAsync(brand.ImageUrl);
                    if (deleted)
                    {
                        _logger.LogInformation("Old image deleted: {ImageUrl}", brand.ImageUrl);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete old image: {ImageUrl}", brand.ImageUrl);
                    }
                }

                brand.ImageUrl = await _cloudinaryService.UploadImageAsync(command.Image);
                _logger.LogInformation("Image updated: {ImageUrl}", brand.ImageUrl);
            }

            brand.UpdatedAt = DateTime.UtcNow.AddHours(7);
            await _repository.UpdateAsync(brand);
            _logger.LogInformation("Brand updated: {Id}", brand.Id);

            await _eventStore.SaveEventAsync(brand.Id, "BrandUpdated", new
            {
                brand.Id,
                brand.Name,
                brand.Description,
                brand.ImageUrl
            });

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

            await _eventStore.SaveEventAsync(command.Id, "BrandDeleted", new
            {
                BrandId = command.Id,
                DeletedAt = DateTime.UtcNow
            });

            return true;
        }
    }
}