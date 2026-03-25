using DTOs;
using static DTOs.ProductDto;

namespace ProductService.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
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
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}