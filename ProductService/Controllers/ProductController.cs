using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase

    {
        private readonly IService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách sản phẩm thành công",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy danh sách sản phẩm",
                    Data = null
                });
            }
        }

        // GET: api/products/brand/5
        [HttpGet("brand/{brandId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByBrand(int brandId)
        {
            try
            {
                var products = await _productService.GetByBrandIdAsync(brandId);

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 200,
                    Message = products.Any()
                        ? $"Lấy sản phẩm theo thương hiệu thành công"
                        : $"Không tìm thấy sản phẩm nào cho thương hiệu ID {brandId}",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by brand {BrandId}", brandId);

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy sản phẩm theo thương hiệu",
                    Data = null
                });
            }
        }

        // GET: api/products/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetByCategoryIdAsync(categoryId);

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 200,
                    Message = products.Any()
                        ? $"Lấy sản phẩm theo danh mục thành công"
                        : $"Không tìm thấy sản phẩm nào cho danh mục ID {categoryId}",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);

                return Ok(new ApiResponse<IEnumerable<ProductDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy sản phẩm theo danh mục",
                    Data = null
                });
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                {
                    return Ok(new ApiResponse<ProductDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy sản phẩm với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin sản phẩm thành công",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy thông tin sản phẩm",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductDto dto)
        {
            try
            {
                var product = await _productService.CreateAsync(dto);

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 201,
                    Message = "Tạo sản phẩm thành công",
                    Data = product
                });
            }
            catch (InvalidOperationException ex)
            {
                // Known business logic errors (brand not found, category not found, etc.)
                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi tạo sản phẩm",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, UpdateProductDto dto)
        {
            try
            {
                var product = await _productService.UpdateAsync(id, dto);

                if (product == null)
                {
                    return Ok(new ApiResponse<ProductDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy sản phẩm với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 200,
                    Message = "Cập nhật sản phẩm thành công",
                    Data = product
                });
            }
            catch (InvalidOperationException ex)
            {
                // Known business logic errors
                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);

                return Ok(new ApiResponse<ProductDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi cập nhật sản phẩm",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                var deleted = await _productService.DeleteAsync(id);

                if (!deleted)
                {
                    return Ok(new ApiResponse
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy sản phẩm với ID {id}"
                    });
                }

                return Ok(new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Xóa sản phẩm thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);

                return Ok(new ApiResponse
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi xóa sản phẩm"
                });
            }
        }
    }
}