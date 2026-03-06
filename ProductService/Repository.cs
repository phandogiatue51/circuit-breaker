using Microsoft.EntityFrameworkCore;

namespace ProductService
{
    public class Repository

    {
        private readonly ProductDbContext _context;

        public Repository(ProductDbContext context)
        {
            _context = context;
        }

        // Get all products with includes
        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get by ID with includes
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Get products by brand
        public async Task<List<Product>> GetByBrandIdAsync(int brandId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .Where(p => p.BrandId == brandId)
                .ToListAsync();
        }

        // Get products by category
        public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
                .ToListAsync();
        }
        
        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        // Update product
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // Delete product
        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        // Check if exists
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}