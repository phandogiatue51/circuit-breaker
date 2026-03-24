using DTOs;
using BrandService.Mappers;

namespace BrandService.Queries
{
    public class BrandQueryHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<BrandQueryHandler> _logger;

        public BrandQueryHandler(Repository repository, ILogger<BrandQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// QUERY: Lấy tất cả thương hiệu
        /// </summary>
        public async Task<IEnumerable<BrandDto>> Handle(GetAllBrandsQuery query)
        {
            _logger.LogInformation("Handling GetAllBrandsQuery");

            var brands = await _repository.GetAllAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                brands = query.SortDescending
                    ? brands.OrderByDescending(p => GetPropertyValue(p, query.SortBy)).ToList()
                    : brands.OrderBy(p => GetPropertyValue(p, query.SortBy)).ToList();
            }

            // Apply pagination
            if (query.Page.HasValue && query.PageSize.HasValue)
            {
                brands = brands
                    .Skip((query.Page.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .ToList();
            }

            return brands.Select(BrandMapper.ToDto);
        }

        /// <summary>
        /// QUERY: Lấy thương hiệu theo ID
        /// </summary>
        public async Task<BrandDto?> Handle(GetBrandQuery query)
        {
            _logger.LogInformation("Handling GetProductQuery for id: {Id}", query.Id);

            var product = await _repository.GetByIdAsync(query.Id);
            return product != null ? BrandMapper.ToDto(product) : null;
        }

        private object GetPropertyValue(Brand brand, string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "name" => brand.Name,
                "description" => brand.Description,
                "createdat" => brand.CreatedAt,
                "updatedat" => brand.UpdatedAt ?? DateTime.MinValue,
                "id" => brand.Id,
                _ => brand.Id
            };
        }
    }
}