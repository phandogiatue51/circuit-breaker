using Clients;
using DTOs;
using DTOs.Exceptions;
using ProductService.Mappers;

namespace ProductService.Commands
{
    public class ProductCommandHandler
    {
        private readonly Repository _repository;
        private readonly EventStoreService _eventStore;  
        private readonly BrandServiceClient _brandClient;
        private readonly CategoryServiceClient _categoryClient;
        private readonly ILogger<ProductCommandHandler> _logger;

        public ProductCommandHandler(
            Repository repository,
                    EventStoreService eventStore, 
            BrandServiceClient brandClient,
            CategoryServiceClient categoryClient,
            ILogger<ProductCommandHandler> logger)
        {
            _repository = repository;
            _brandClient = brandClient;
            _eventStore = eventStore;  
            _categoryClient = categoryClient;
            _logger = logger;
        }

        /// <summary>
        /// COMMAND: Tạo sản phẩm mới
        /// </summary>
        public async Task<ProductDto> Handle(CreateProductCommand command)
        {
            _logger.LogInformation("Handling CreateProductCommand: {Name}", command.Name);

            // Validate brand
            var brand = await _brandClient.GetByIdAsync(command.BrandId);
            if (brand == null)
            {
                throw new BadRequestException($"Brand with ID {command.BrandId} not found", "BRAND_NOT_FOUND");
            }

            // Validate categories
            var categories = await _categoryClient.GetByIdsAsync(command.CategoryIds);
            if (categories.Count != command.CategoryIds.Count)
            {
                var foundIds = categories.Select(c => c.Id).ToList();
                var missingIds = command.CategoryIds.Except(foundIds);
                throw new BadRequestException($"Categories not found: {string.Join(", ", missingIds)}", "CATEGORIES_NOT_FOUND");
            }

            // Create product
            var product = new Product
            {
                Name = command.Name,
                Description = command.Description,
                Price = command.Price,
                Origin = command.Origin,
                Material = command.Material,
                BrandId = brand.Id,
                BrandName = brand.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
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

            await _eventStore.SaveEventAsync(product.Id, "ProductCreated", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.BrandId,
                CategoryIds = command.CategoryIds
            });

            _logger.LogInformation("Product created: {Id} - {Name}", product.Id, product.Name);

            return ProductMapper.ToDto(product);
        }

        /// <summary>
        /// COMMAND: Cập nhật sản phẩm
        /// </summary>
        public async Task<ProductDto?> Handle(UpdateProductCommand command, int id)
        {
            _logger.LogInformation("Handling UpdateProductCommand for id: {Id}", id);

            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            // Update basic fields
            if (!string.IsNullOrWhiteSpace(command.Name))
                product.Name = command.Name;

            if (!string.IsNullOrWhiteSpace(command.Description))
                product.Description = command.Description;

            if (command.Price.HasValue)
                product.Price = command.Price.Value;

            if (!string.IsNullOrWhiteSpace(command.Origin))
                product.Origin = command.Origin;

            if (!string.IsNullOrWhiteSpace(command.Material))
                product.Material = command.Material;

            if (command.IsActive.HasValue)
                product.IsActive = command.IsActive.Value;

            // Update brand if changed
            if (command.BrandId.HasValue && command.BrandId.Value != product.BrandId)
            {
                var brand = await _brandClient.GetByIdAsync(command.BrandId.Value);
                if (brand == null)
                {
                    throw new BadRequestException($"Brand with ID {command.BrandId} not found", "BRAND_NOT_FOUND");
                }
                product.BrandId = brand.Id;
                product.BrandName = brand.Name;
            }

            // Update categories if changed
            if (command.CategoryIds != null)
            {
                if (command.CategoryIds.Any())
                {
                    var validCategoryIds = command.CategoryIds.Where(id => id > 0).ToList();
                    if (validCategoryIds.Any())
                    {
                        var categories = await _categoryClient.GetByIdsAsync(validCategoryIds);
                        if (categories.Count != validCategoryIds.Count)
                        {
                            var foundIds = categories.Select(c => c.Id).ToList();
                            var missingIds = validCategoryIds.Except(foundIds);
                            throw new BadRequestException($"Categories not found: {string.Join(", ", missingIds)}", "CATEGORIES_NOT_FOUND");
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

            product.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(product);

            await _eventStore.SaveEventAsync(id, "ProductUpdated", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.BrandId,
                UpdatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Product updated: {Id}", product.Id);

            return ProductMapper.ToDto(product);
        }

        /// <summary>
        /// COMMAND: Xóa sản phẩm
        /// </summary>
        public async Task<bool> Handle(DeleteProductCommand command)
        {
            _logger.LogInformation("Handling DeleteProductCommand for id: {Id}", command.Id);

            var exists = await _repository.ExistsAsync(command.Id);
            if (!exists) return false;

            await _repository.DeleteAsync(command.Id);

            await _eventStore.SaveEventAsync(command.Id, "ProductDeleted", new
            {
                ProductId = command.Id,
                DeletedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Product deleted: {Id}", command.Id);

            return true;
        }
    }
}