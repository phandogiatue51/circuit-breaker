using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandService.Queries;

namespace BrandService.Controllers
{
    [ApiController]
    [Route("api/queries/brands")]
    [Authorize]
    public class BrandQueryController : ControllerBase
    {
        private readonly BrandQueryHandler _queryHandler;
        private readonly ILogger<BrandQueryController> _logger;

        public BrandQueryController(BrandQueryHandler queryHandler, ILogger<BrandQueryController> logger)
        {
            _queryHandler = queryHandler;
            _logger = logger;
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
                    products, path, "Lấy danh sách thương hiệu thành công"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Có lỗi khi lấy danh sách thương hiệu", path, "INTERNAL_ERROR"));
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
                    return NotFound(ApiResponse<BrandDto>.Error(404, $"Không tìm thấy thương hiệu với ID {id}", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<BrandDto>.Success(product, path, "Lấy thông tin thương hiệu thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Có lỗi khi lấy thông tin thương hiệu", path, "INTERNAL_ERROR"));
            }
        }
    }
}