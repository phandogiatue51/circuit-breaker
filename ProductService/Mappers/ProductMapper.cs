using DTOs;
using static DTOs.ProductDto;

namespace ProductService.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product, IReadOnlyDictionary<int, string>? categoryNames = null)
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
                CategoryName = GetCategoryName(pc.CategoryId, pc.CategoryName, categoryNames)
            }).ToList(),
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static string GetCategoryName(int categoryId, string fallbackName, IReadOnlyDictionary<int, string>? categoryNames)
    {
        if (categoryNames != null && categoryNames.TryGetValue(categoryId, out var resolvedName) && !string.IsNullOrWhiteSpace(resolvedName))
        {
            return resolvedName;
        }

        return fallbackName;
    }
}