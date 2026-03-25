namespace BrandService.Commands
{
    public class UpdateBrandCommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
