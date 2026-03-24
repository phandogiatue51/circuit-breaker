using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BrandService.Commands;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/commands/products")]
    [Authorize(Roles = "Admin")]
    public class BrandCommandController : ControllerBase
    {
        private readonly CategoryCommandHandler _commandHandler;
        private readonly ILogger<BrandCommandController> _logger;

        public BrandCommandController(CategoryCommandHandler commandHandler, ILogger<BrandCommandController> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Create([FromBody] CreateCategoryCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = product.Id },
                    ApiResponse<BrandDto>.Success(product, path, "Tạo sản phẩm thành công")
                );
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<BrandDto>.Error(
                    ex.StatusCode, ex.Message, path, ex.ErrorCode
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<BrandDto>.Error(500, "Có lỗi khi tạo sản phẩm", path));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Update(int id, [FromBody] UpdateCategoryCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command, id);

                if (product == null)
                {
                    return NotFound(ApiResponse<BrandDto>.Error(404, $"Không tìm thấy sản phẩm với ID {id}", path));
                }

                return Ok(ApiResponse<BrandDto>.Success(product, path, "Cập nhật sản phẩm thành công"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<BrandDto>.Error(ex.StatusCode, ex.Message, path, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, ApiResponse<BrandDto>.Error(500, "Có lỗi khi cập nhật sản phẩm", path));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var command = new DeleteCategoryCommand { Id = id };
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