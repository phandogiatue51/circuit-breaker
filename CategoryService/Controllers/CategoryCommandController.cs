using CategoryService.Commands;
using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.Controllers
{
    [ApiController]
    [Route("api/commands/categories")]
    [Authorize(Roles = "Admin")]
    public class CategoryCommandController : ControllerBase
    {
        private readonly CategoryCommandHandler _commandHandler;
        private readonly ILogger<CategoryCommandController> _logger;

        public CategoryCommandController(CategoryCommandHandler commandHandler, ILogger<CategoryCommandController> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var category = await _commandHandler.Handle(command);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = category.Id },
                    ApiResponse<CategoryDto>.Success(category, path, "Tạo phân loại thành công")
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
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, ApiResponse<CategoryDto>.Error(500, "Có lỗi khi tạo phân loại", path, "NOT_FOUND"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var category = await _commandHandler.Handle(command, id);

                if (category == null)
                {
                    return NotFound(ApiResponse<CategoryDto>.Error(404, $"Không tìm thấy phân loại với ID {id}", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<CategoryDto>.Success(category, path, "Cập nhật phân loại thành công"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<CategoryDto>.Error(ex.StatusCode, ex.Message, path, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);
                return StatusCode(500, ApiResponse<BrandDto>.Error(500, "Có lỗi khi cập nhật phân loại", path, "INTERNAL_ERROR"));
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
                    return NotFound(ApiResponse.Error(404, $"Không tìm thấy phân loại với ID {id}", path));
                }

                return Ok(ApiResponse.Success(path, "Xóa phân loại thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                return StatusCode(500, ApiResponse.Error(500, "Có lỗi khi xóa phân loại", path));
            }
        }
    }
}