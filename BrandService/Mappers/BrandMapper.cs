using DTOs;

namespace BrandService.Mappers;

public static class BrandMapper
{
    public static BrandDto ToDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt,
            ImageUrl = brand.ImageUrl,
        };
    }
}