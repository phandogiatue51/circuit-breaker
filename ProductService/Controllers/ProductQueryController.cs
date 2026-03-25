using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Queries;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/queries/products")]
    public class ProductQueryController : ControllerBase
    {
        private readonly ProductQueryHandler _queryHandler;
        private readonly EventStoreService _eventStoreService;
        private readonly ILogger<ProductQueryController> _logger;

        public ProductQueryController(ProductQueryHandler queryHandler, ILogger<ProductQueryController> logger, 
            EventStoreService eventStoreService)
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
                var query = new GetAllProductsQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDescending = sortDesc
                };

                var products = await _queryHandler.Handle(query);

                return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(
                    products, path, "Products retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Error getting all products", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetProductQuery { Id = id };
                var product = await _queryHandler.Handle(query);

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductDto>.Error(404, $"Product with Id {id} not found", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<ProductDto>.Success(product, path, "Product retrieved successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Error getting product", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("brand/{brandId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByBrand(int brandId)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetProductsByBrandQuery { BrandId = brandId };
                var products = await _queryHandler.Handle(query);

                return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(
                    products, path, products.Any() ? "Product retrieved by brand successfully!" : "There is no product"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by brand {BrandId}", brandId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Error getting products by brand", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(int categoryId)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetProductsByCategoryQuery { CategoryId = categoryId };
                var products = await _queryHandler.Handle(query);

                return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(
                    products, path, products.Any() ? "Product retrieved by category successfully!" : "There is no product"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Error getting products by category", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("{id}/events")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<ProductEvent>>>> GetEvents(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var events = await _eventStoreService.GetEventsAsync(id);

                return Ok(ApiResponse<List<ProductEvent>>.Success(
                    events,
                    path,
                    events.Any() ? "Product history retrieved successfully!" : "Product history is empty"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for product {Id}", id);
                return StatusCode(500, ApiResponse<List<ProductEvent>>.Error(500, "Error getting events for product", path, "INTERNAL_ERROR"));
            }
        }
    }
}