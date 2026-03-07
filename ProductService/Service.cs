using Clients;
using DTOs;

namespace ProductService
{
    public class Service : IService
    {
        private readonly Repository _repository;
        private readonly BrandServiceClient _brandClient;
        private readonly CategoryServiceClient _categoryClient;
        private readonly ILogger<Service> _logger;

        public Service(Repository repository, BrandServiceClient brandClient, CategoryServiceClient categoryClient, ILogger<Service> logger)
        {
            _repository = repository;
            _brandClient = brandClient;
            _categoryClient = categoryClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetByBrandIdAsync(int brandId)
        {
            var products = await _repository.GetByBrandIdAsync(brandId);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId)
        {
            var products = await _repository.GetByCategoryIdAsync(categoryId);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            BrandDto? brand;
            try
            {
                brand = await _brandClient.GetByIdAsync(dto.BrandId);

                if (brand == null)
                {
                    throw new InvalidOperationException($"Brand with ID {dto.BrandId} not found");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Brand service connection failed for ID {BrandId}", dto.BrandId);
                throw new InvalidOperationException("Brand service is currently unavailable. Please try again later.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Brand service timeout for ID {BrandId}", dto.BrandId);
                throw new InvalidOperationException("Brand service is taking too long to respond. Please try again.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error verifying brand {BrandId}", dto.BrandId);
                throw new InvalidOperationException("An error occurred while verifying the brand");
            }

            List<CategoryDto> categories;
            try
            {
                categories = await _categoryClient.GetByIdsAsync(dto.CategoryIds);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Category service connection failed for IDs: {CategoryIds}", string.Join(",", dto.CategoryIds));
                throw new InvalidOperationException("Category service is currently unavailable. Please try again later.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Category service timeout for IDs: {CategoryIds}", string.Join(",", dto.CategoryIds));
                throw new InvalidOperationException("Category service is taking too long to respond. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying categories: {CategoryIds}", string.Join(",", dto.CategoryIds));
                throw new InvalidOperationException("An error occurred while verifying the categories");
            }

            if (categories.Count != dto.CategoryIds.Count)
            {
                var foundIds = categories.Select(c => c.Id).ToList();
                var missingIds = dto.CategoryIds.Except(foundIds);
                throw new InvalidOperationException($"Categories not found: {string.Join(", ", missingIds)}");
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Origin = dto.Origin,
                Material = dto.Material,
                BrandId = brand.Id,
                BrandName = brand.Name,
                ProductCategories = new List<ProductCategory>()
            };

            foreach (var cat in categories)
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = cat.Id,
                    CategoryName = cat.Name
                });
            }

            await _repository.CreateAsync(product);
            _logger.LogInformation("Created product: {ProductName} (ID: {ProductId})", product.Name, product.Id);

            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                product.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                product.Description = dto.Description;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (!string.IsNullOrWhiteSpace(dto.Origin))
                product.Origin = dto.Origin;

            if (!string.IsNullOrWhiteSpace(dto.Material))
                product.Material = dto.Material;

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            if (dto.BrandId.HasValue && dto.BrandId.Value != product.BrandId)
            {
                var brand = await _brandClient.GetByIdAsync(dto.BrandId.Value);
                if (brand == null)
                {
                    throw new InvalidOperationException($"Brand with ID {dto.BrandId} not found");
                }
                product.BrandId = brand.Id;
                product.BrandName = brand.Name;
            }

            if (dto.CategoryIds != null)  
            {
                if (dto.CategoryIds.Any())
                {
                    var validCategoryIds = dto.CategoryIds
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value) 
                        .ToList();

                    if (validCategoryIds.Any())
                    {
                        var categories = await _categoryClient.GetByIdsAsync(validCategoryIds);

                        if (categories.Count != validCategoryIds.Count)
                        {
                            var foundIds = categories.Select(c => c.Id).ToList();
                            var missingIds = validCategoryIds.Except(foundIds);
                            throw new InvalidOperationException($"Categories not found: {string.Join(", ", missingIds)}");
                        }

                        product.ProductCategories.Clear();

                        foreach (var cat in categories)
                        {
                            product.ProductCategories.Add(new ProductCategory
                            {
                                CategoryId = cat.Id,
                                CategoryName = cat.Name
                            });
                        }
                    }
                    else
                    {
                        product.ProductCategories.Clear();
                    }
                }
                else
                {
                    product.ProductCategories.Clear();
                }
            }

            await _repository.UpdateAsync(product);
            _logger.LogInformation("Updated product ID: {ProductId}", product.Id);

            return MapToDto(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists) return false;

            await _repository.DeleteAsync(id);
            _logger.LogInformation("Deleted product ID: {ProductId}", id);
            return true;
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Origin = product.Origin,
                Material = product.Material,
                IsActive = product.IsActive,
                BrandId = product.BrandId,
                BrandName = product.BrandName,
                Categories = product.ProductCategories.Select(pc => new CategoryInfoDto
                {
                    CategoryId = pc.CategoryId,
                    CategoryName = pc.CategoryName
                }).ToList(),
                CreatedAt = product.CreatedAt
            };
        }
    }
}