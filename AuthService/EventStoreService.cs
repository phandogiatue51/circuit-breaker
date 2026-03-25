using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AuthService
{
    public class EventStoreService
    {
        private readonly AccountDbContext _context;
        private readonly ILogger<EventStoreService> _logger;

        public EventStoreService(AccountDbContext context, ILogger<EventStoreService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lưu event
        public async Task SaveEventAsync(int authId, string eventType, object payload)
        {
            var authEvent = new AuthEvent
            {
                AuthId = authId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuthEvents.AddAsync(authEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event saved: {EventType} for auth {AuthId}", eventType, authId);
        }

        // Lấy tất cả events của 1 auth
        public async Task<List<AuthEvent>> GetEventsAsync(int authId)
        {
            return await _context.AuthEvents
                .Where(e => e.AuthId == authId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}