using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Commands;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/commands/products")]
    [Authorize(Roles = "Admin")]
    public class ProductCommandController : ControllerBase
    {
        private readonly ProductCommandHandler _commandHandler;
        private readonly ILogger<ProductCommandController> _logger;

        public ProductCommandController(ProductCommandHandler commandHandler, ILogger<ProductCommandController> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = product.Id },
                    ApiResponse<ProductDto>.Success(product, path, "Tạo sản phẩm thành công")
                );
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<ProductDto>.Error(
                    ex.StatusCode, ex.Message, path, ex.ErrorCode
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Có lỗi khi tạo sản phẩm", path, "INTERNAL_ERROR"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command, id);

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductDto>.Error(404, $"Không tìm thấy sản phẩm với ID {id}", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<ProductDto>.Success(product, path, "Cập nhật sản phẩm thành công"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<ProductDto>.Error(ex.StatusCode, ex.Message, path, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Có lỗi khi cập nhật sản phẩm", path, "INTERNAL_ERROR"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var command = new DeleteProductCommand { Id = id };
                var deleted = await _commandHandler.Handle(command);

                if (!deleted)
                {
                    return NotFound(ApiResponse.Error(404, $"Không tìm thấy sản phẩm với ID {id}", path));
                }

                return Ok(ApiResponse.Success(path, "Xóa sản phẩm thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, ApiResponse.Error(500, "Có lỗi khi xóa sản phẩm", path));
            }
        }
    }
}