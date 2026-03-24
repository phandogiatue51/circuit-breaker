using DTOs.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AuthService;
public class Repository
{
    private readonly AccountDbContext _context;

    public Repository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Account> GetByIdAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
            throw new NotFoundException("Account", id); // ⭐ Throw exception

        return account;
    }

    public async Task CreateAsync(Account account)
    {
        try
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Kiểm tra duplicate email
            if (ex.InnerException?.Message.Contains("duplicate") == true)
                throw new ConflictException("Email đã tồn tại trong hệ thống", "DUPLICATE_EMAIL");

            throw; // Ném lại exception khác
        }
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var account = await GetByIdAsync(id);
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Accounts.AnyAsync(u => u.Email == email);
    }
}