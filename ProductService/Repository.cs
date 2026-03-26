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

        public async Task<List<Product>> GetAllAsync()
        {
            try
            {
                return await _context.Products
                    .Include(p => p.ProductCategories)
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }
            catch
            {
                return new List<Product>();
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetByBrandIdAsync(int brandId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .Where(p => p.BrandId == brandId)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
                .OrderBy(p => p.Id)
                .ToListAsync();
        }
        
        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}