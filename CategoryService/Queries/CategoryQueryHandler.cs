using CategoryService.Mappers;
using DTOs;

namespace CategoryService.Queries
{
    public class CategoryQueryHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<CategoryQueryHandler> _logger;

        public CategoryQueryHandler(Repository repository, ILogger<CategoryQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// QUERY: Lấy tất cả phân loại
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery query)
        {
            _logger.LogInformation("Handling GetAllCategoriesQuery");

            var categories = await _repository.GetAllAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                categories = query.SortDescending
                    ? categories.OrderByDescending(p => GetPropertyValue(p, query.SortBy)).ToList()
                    : categories.OrderBy(p => GetPropertyValue(p, query.SortBy)).ToList();
            }

            // Apply pagination
            if (query.Page.HasValue && query.PageSize.HasValue)
            {
                categories = categories
                    .Skip((query.Page.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .ToList();
            }

            return categories.Select(CategoryMapper.ToDto);
        }

        /// <summary>
        /// QUERY: Lấy phân loại theo ID
        /// </summary>
        public async Task<CategoryDto?> Handle(GetCategoryQuery query)
        {
            _logger.LogInformation("Handling GetCategoryQuery for id: {Id}", query.Id);

            var category = await _repository.GetByIdAsync(query.Id);
            return category != null ? CategoryMapper.ToDto(category) : null;
        }

        /// <summary>
        /// QUERY: Lấy nhiều phân loại theo danh sách ID
        /// </summary>
        public async Task<List<CategoryDto>> Handle(GetCategoriesByIdsQuery query)
        {
            _logger.LogInformation("Handling GetCategoriesByIdsQuery for ids: {Ids}", string.Join(",", query.Ids));

            if (query.Ids == null || !query.Ids.Any())
            {
                return new List<CategoryDto>();
            }

            var categories = await _repository.GetByIdsAsync(query.Ids);

            if (categories == null || !categories.Any())
            {
                return new List<CategoryDto>();
            }

            return categories.Select(CategoryMapper.ToDto).ToList();
        }

        private object GetPropertyValue(Category category, string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "name" => category.Name,
                "description" => category.Description,
                "createdat" => category.CreatedAt,
                "updatedat" => category.UpdatedAt ?? DateTime.MinValue,
                "id" => category.Id,
                _ => category.Id
            };
        }
    }
}