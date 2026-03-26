using Microsoft.EntityFrameworkCore;

namespace CategoryService
{
    public class Repository
    {
        private readonly CategoryDbContext _context;

        public Repository(CategoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            try
            {
                return await _context.Categories
                    .OrderBy(b => b.Name)
                    .ToListAsync();
            }
            catch
            {
                return new List<Category>();
            }
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetByIdsAsync(List<int> ids, bool includeInactive = true)
        {
            var query = _context.Categories.Where(c => ids.Contains(c.Id));

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query.ToListAsync();
        }

        public async Task CreateAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(b => b.Id == id);
        }
    }
}