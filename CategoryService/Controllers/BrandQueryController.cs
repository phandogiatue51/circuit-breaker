using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Queries;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/queries/products")]
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

                return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(
                    products, path, "Lấy danh sách sản phẩm thành công"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Có lỗi khi lấy danh sách sản phẩm", path));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetBrandQuery { Id = id };
                var product = await _queryHandler.Handle(query);

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductDto>.Error(404, $"Không tìm thấy sản phẩm với ID {id}", path));
                }

                return Ok(ApiResponse<ProductDto>.Success(product, path, "Lấy thông tin sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Có lỗi khi lấy thông tin sản phẩm", path));
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
                    products, path, products.Any() ? "Lấy sản phẩm theo thương hiệu thành công" : "Không có sản phẩm"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by brand {BrandId}", brandId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Có lỗi khi lấy sản phẩm theo thương hiệu", path));
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
                    products, path, products.Any() ? "Lấy sản phẩm theo danh mục thành công" : "Không có sản phẩm"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Error(500, "Có lỗi khi lấy sản phẩm theo danh mục", path));
            }
        }
    }
}