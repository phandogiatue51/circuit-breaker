using Microsoft.EntityFrameworkCore;

namespace BrandService
{
    public class Repository
    {
        private readonly BrandDbContext _context;

        public Repository(BrandDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            try
            {
                return await _context.Brands
                    .OrderBy(b => b.Id)
                    .ToListAsync();
            }
            catch
            {
                return new List<Brand>();
            }
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands.FindAsync(id);
        }

        public async Task CreateAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await GetByIdAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Brands.AnyAsync(b => b.Id == id);
        }
    }
}