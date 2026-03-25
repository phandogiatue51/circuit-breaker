using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CategoryService
{
    public class EventStoreService
    {
        private readonly CategoryDbContext _context;
        private readonly ILogger<EventStoreService> _logger;

        public EventStoreService(CategoryDbContext context, ILogger<EventStoreService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lưu event
        public async Task SaveEventAsync(int categoryId, string eventType, object payload)
        {
            var categoryEvent = new CategoryEvent
            {
                CategoryId = categoryId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            };

            await _context.CategoryEvents.AddAsync(categoryEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event saved: {EventType} for category {CategoryId}", eventType, categoryId);
        }

        // Lấy tất cả events của 1 category
        public async Task<List<CategoryEvent>> GetEventsAsync(int categoryId)
        {
            return await _context.CategoryEvents
                .Where(e => e.CategoryId == categoryId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}