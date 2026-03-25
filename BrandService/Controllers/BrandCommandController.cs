using DTOs;
using DTOs.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandService.Commands;

namespace BrandService.Controllers
{
    [ApiController]
    [Route("api/commands/brands")]
    [Authorize(Roles = "Admin")]
    public class BrandCommandController : ControllerBase
    {
        private readonly BrandCommandHandler _commandHandler;
        private readonly ILogger<BrandCommandController> _logger;

        public BrandCommandController(BrandCommandHandler commandHandler, ILogger<BrandCommandController> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Create([FromForm] CreateBrandCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
                if (!allowedTypes.Contains(command.Image.ContentType))
                {
                    throw new BadRequestException("Only JPEG, PNG, WEBP images are allowed!", "INVALID_FILE_TYPE");
                }

                // Validate file size (max 5MB)
                if (command.Image.Length > 5 * 1024 * 1024)
                {
                    throw new BadRequestException("Image size must be less than 5MB!", "FILE_TOO_LARGE");
                }

                var brand = await _commandHandler.Handle(command);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = brand.Id },
                    ApiResponse<BrandDto>.Success(brand, path, "Brand created successfully!")
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
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, ApiResponse<BrandDto>.Error(500, "Error when creating brand", path, "INTERNAL_ERROR"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Update(int id, [FromForm] UpdateBrandCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                // Validate file if uploaded
                if (command.Image != null)
                {
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
                    if (!allowedTypes.Contains(command.Image.ContentType))
                    {
                        throw new BadRequestException("Only JPEG, PNG, WEBP images are allowed!", "INVALID_FILE_TYPE");
                    }

                    if (command.Image.Length > 5 * 1024 * 1024)
                    {
                        throw new BadRequestException("Image size must be less than 5MB!", "FILE_TOO_LARGE");
                    }
                }

                var brand = await _commandHandler.Handle(command, id);

                if (brand == null)
                {
                    return NotFound(ApiResponse<BrandDto>.Error(404, $"Brand with Id {id} not found!", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<BrandDto>.Success(brand, path, "Brand updated successfully!"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<BrandDto>.Error(ex.StatusCode, ex.Message, path, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {Id}", id);
                return StatusCode(500, ApiResponse<BrandDto>.Error(500, "Error when updating brand", path, "INTERNAL_ERROR"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var command = new DeleteBrandCommand { Id = id };
                var deleted = await _commandHandler.Handle(command);

                if (!deleted)
                {
                    return NotFound(ApiResponse.Error(404, $"Brand with Id {id} not found!", path));
                }

                return Ok(ApiResponse.Success(path, "Brand deleted successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {Id}", id);
                return StatusCode(500, ApiResponse.Error(500, "Error when deleting brand!", path));
            }
        }
    }
}