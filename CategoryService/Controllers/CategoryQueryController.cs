using CategoryService.Queries;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.Controllers
{
    [ApiController]
    [Route("api/queries/categories")]
    public class CategoryQueryController : ControllerBase
    {
        private readonly CategoryQueryHandler _queryHandler;
        private readonly EventStoreService _eventStoreService;
        private readonly ILogger<CategoryQueryController> _logger;

        public CategoryQueryController(CategoryQueryHandler queryHandler, ILogger<CategoryQueryController> logger, EventStoreService eventStoreService)
        {
            _queryHandler = queryHandler;
            _logger = logger;
            _eventStoreService = eventStoreService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDesc = false)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetAllCategoriesQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDescending = sortDesc
                };

                var categories = await _queryHandler.Handle(query);

                return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(
                    categories, path, "Category retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Error when retrieving category", path, "INTERNAL_ERROR"
));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetCategoryQuery { Id = id };
                var category = await _queryHandler.Handle(query);

                if (category == null)
                {
                    return NotFound(ApiResponse<CategoryDto>.Error(404, $"Category with Id  {id}  not found", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<CategoryDto>.Success(category, path, "Category retrieved successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<CategoryDto>.Error(500, "Error when retrieving category", path, "INTERNAL_ERROR"));
            }
        }

        [HttpGet("by-ids")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetByIds([FromQuery] List<int> ids)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                if (ids == null || !ids.Any())
                {
                    return BadRequest(ApiResponse<List<CategoryDto>>.Error(
                        400,
                        "IDs cannot be empty",
                        path,
                        "INVALID_REQUEST"
                    ));
                }

                var query = new GetCategoriesByIdsQuery { Ids = ids };
                var categories = await _queryHandler.Handle(query);

                return Ok(ApiResponse<List<CategoryDto>>.Success(
                    categories,
                    path,
                    $"Categories {categories.Count} retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories by IDs: {Ids}", string.Join(",", ids));
                return StatusCode(500, ApiResponse<List<CategoryDto>>.Error(
                    500,
                    "Error when retrieving Category IDs",
                    path,
                    "INTERNAL_ERROR"
                ));
            }
        }

        [HttpGet("{id}/events")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoryEvent>>>> GetEvents(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var events = await _eventStoreService.GetEventsAsync(id);

                return Ok(ApiResponse<List<CategoryEvent>>.Success(
                    events,
                    path,
                    events.Any() ? "Category history retrieved successfully!" : "Category history is empty"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for category {Id}", id);
                return StatusCode(500, ApiResponse<List<CategoryEvent>>.Error(500, "Error when retrieving category history", path, "INTERNAL_ERROR"));
            }
        }
    }
}