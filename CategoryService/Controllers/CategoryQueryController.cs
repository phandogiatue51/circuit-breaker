using CategoryService.Queries;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.Controllers
{
    [ApiController]
    [Route("api/queries/categories")]
    [Authorize]
    public class CategoryQueryController : ControllerBase
    {
        private readonly CategoryQueryHandler _queryHandler;
        private readonly ILogger<CategoryQueryController> _logger;

        public CategoryQueryController(CategoryQueryHandler queryHandler, ILogger<CategoryQueryController> logger)
        {
            _queryHandler = queryHandler;
            _logger = logger;
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
                    categories, path, "Lấy danh sách phân loại thành công"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Có lỗi khi lấy danh sách phân loại", path, "INTERNAL_ERROR"
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
                    return NotFound(ApiResponse<CategoryDto>.Error(404, $"Không tìm thấy phân loại với ID {id}", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<CategoryDto>.Success(category, path, "Lấy thông tin phân loại thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<CategoryDto>.Error(500, "Có lỗi khi lấy thông tin phân loại", path, "INTERNAL_ERROR"));
            }
        }
    }
}