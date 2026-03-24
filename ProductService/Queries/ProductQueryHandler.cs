using DTOs;

namespace ProductService.Queries
{
    public class ProductQueryHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<ProductQueryHandler> _logger;

        public ProductQueryHandler(Repository repository, ILogger<ProductQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// QUERY: Lấy tất cả sản phẩm
        /// </summary>
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery query)
        {
            _logger.LogInformation("Handling GetAllProductsQuery");

            var products = await _repository.GetAllAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                products = query.SortDescending
                    ? products.OrderByDescending(p => GetPropertyValue(p, query.SortBy)).ToList()
                    : products.OrderBy(p => GetPropertyValue(p, query.SortBy)).ToList();
            }

            // Apply pagination
            if (query.Page.HasValue && query.PageSize.HasValue)
            {
                products = products
                    .Skip((query.Page.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .ToList();
            }

            return products.Select(ProductMapper.ToDto);
        }

        /// <summary>
        /// QUERY: Lấy sản phẩm theo ID
        /// </summary>
        public async Task<ProductDto?> Handle(GetProductQuery query)
        {
            _logger.LogInformation("Handling GetProductQuery for id: {Id}", query.Id);

            var product = await _repository.GetByIdAsync(query.Id);
            return product != null ? ProductMapper.ToDto(product) : null;
        }

        /// <summary>
        /// QUERY: Lấy sản phẩm theo Brand ID
        /// </summary>
        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByBrandQuery query)
        {
            _logger.LogInformation("Handling GetProductsByBrandQuery for brand: {BrandId}", query.BrandId);

            var products = await _repository.GetByBrandIdAsync(query.BrandId);
            return products.Select(ProductMapper.ToDto);
        }

        /// <summary>
        /// QUERY: Lấy sản phẩm theo Category ID
        /// </summary>
        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery query)
        {
            _logger.LogInformation("Handling GetProductsByCategoryQuery for category: {CategoryId}", query.CategoryId);

            var products = await _repository.GetByCategoryIdAsync(query.CategoryId);
            return products.Select(ProductMapper.ToDto);
        }

        private object GetPropertyValue(Product product, string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "name" => product.Name,
                "price" => product.Price,
                "createdat" => product.CreatedAt,
                "updatedat" => product.UpdatedAt ?? DateTime.MinValue,
                "id" => product.Id,
                _ => product.Id
            };
        }
    }
}