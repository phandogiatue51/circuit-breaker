using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BrandService
{
    public class EventStoreService
    {
        private readonly BrandDbContext _context;
        private readonly ILogger<EventStoreService> _logger;

        public EventStoreService(BrandDbContext context, ILogger<EventStoreService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lưu event
        public async Task SaveEventAsync(int brandId, string eventType, object payload)
        {
            var brandEvent = new BrandEvent
            {
                BrandId = brandId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            };

            await _context.BrandEvents.AddAsync(brandEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event saved: {EventType} for brand {BrandId}", eventType, brandId);
        }

        // Lấy tất cả events của 1 brand
        public async Task<List<BrandEvent>> GetEventsAsync(int brandId)
        {
            return await _context.BrandEvents
                .Where(e => e.BrandId == brandId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}