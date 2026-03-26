using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandService.Queries;

namespace BrandService.Controllers
{
    [ApiController]
    [Route("api/queries/brands")]
    public class BrandQueryController : ControllerBase
    {
        private readonly BrandQueryHandler _queryHandler;
        private readonly EventStoreService _eventStoreService;
        private readonly ILogger<BrandQueryController> _logger;

        public BrandQueryController(BrandQueryHandler queryHandler, ILogger<BrandQueryController> logger, EventStoreService eventStoreService)
        {
            _queryHandler = queryHandler;
            _logger = logger;
            _eventStoreService = eventStoreService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDesc = false)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetAllBrandsQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDescending = sortDesc
                };

                var products = await _queryHandler.Handle(query);

                return Ok(ApiResponse<IEnumerable<BrandDto>>.Success(
                    products, path, "Brand retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Error when retrieving brand", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetBrandQuery { Id = id };
                var product = await _queryHandler.Handle(query);

                if (product == null)
                {
                    return NotFound(ApiResponse<BrandDto>.Error(404, $"Brand with Id {id} not found", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<BrandDto>.Success(product, path, "Brand retrieved successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Error when retrieving brand", path, "INTERNAL_ERROR"));
            }
        }

        //[HttpGet("{id}")]
        //[AllowAnonymous]
        //public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(int id)
        //{
        //    // Force an error for testing
        //    throw new InvalidOperationException("This is a forced test exception.");
        //}

        [HttpGet("{id}/events")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BrandEvent>>>> GetEvents(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var events = await _eventStoreService.GetEventsAsync(id);

                return Ok(ApiResponse<List<BrandEvent>>.Success(
                    events,
                    path,
                    events.Any() ? "Brand history retrieved successfully!" : "Brand history is empty"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for brand {Id}", id);
                return StatusCode(500, ApiResponse<List<BrandEvent>>.Error(500, "Error when retrieving brand history", path, "INTERNAL_ERROR"));
            }
        }
    }
}