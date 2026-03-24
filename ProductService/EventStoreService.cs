using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ProductService
{
    public class EventStoreService
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<EventStoreService> _logger;

        public EventStoreService(ProductDbContext context, ILogger<EventStoreService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lưu event
        public async Task SaveEventAsync(int productId, string eventType, object payload)
        {
            var productEvent = new ProductEvent
            {
                ProductId = productId,
                EventType = eventType,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductEvents.AddAsync(productEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Event saved: {EventType} for product {ProductId}", eventType, productId);
        }

        // Lấy tất cả events của 1 product
        public async Task<List<ProductEvent>> GetEventsAsync(int productId)
        {
            return await _context.ProductEvents
                .Where(e => e.ProductId == productId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
    }
}