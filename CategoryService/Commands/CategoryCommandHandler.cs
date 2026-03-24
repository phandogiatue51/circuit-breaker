using DTOs;
using CategoryService.Mappers;

namespace CategoryService.Commands
{
    public class CategoryCommandHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<CategoryCommandHandler> _logger;

        public CategoryCommandHandler(
            Repository repository,
            ILogger<CategoryCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// COMMAND: Tạo phân loại mới
        /// </summary>
        public async Task<CategoryDto> Handle(CreateCategoryCommand command)
        {
            _logger.LogInformation("Handling CreateCategoryCommand: {Name}", command.Name);

            // Create category
            var category = new Category
            {
                Name = command.Name,
                Description = command.Description
            };

            await _repository.CreateAsync(category);
            _logger.LogInformation("Category created: {Id} - {Name}", category.Id, category.Name);

            return CategoryMapper.ToDto(category);
        }

        /// <summary>
        /// COMMAND: Cập nhật phân loại
        /// </summary>
        public async Task<CategoryDto?> Handle(UpdateCategoryCommand command, int id)
        {
            _logger.LogInformation("Handling UpdateCategoryCommand for id: {Id}", id);

            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(command.Name))
                category.Name = command.Name;

            if (!string.IsNullOrWhiteSpace(command.Description))
                category.Description = command.Description;

            category.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(category);
            _logger.LogInformation("Category updated: {Id}", category.Id);

            return CategoryMapper.ToDto(category);
        }

        /// <summary>
        /// COMMAND: Xóa thương hiệu
        /// </summary>
        public async Task<bool> Handle(DeleteCategoryCommand command)
        {
            _logger.LogInformation("Handling DeleteCategoryCommand for id: {Id}", command.Id);

            var exists = await _repository.ExistsAsync(command.Id);
            if (!exists) return false;

            await _repository.DeleteAsync(command.Id);
            _logger.LogInformation("Category deleted: {Id}", command.Id);

            return true;
        }
    }
}