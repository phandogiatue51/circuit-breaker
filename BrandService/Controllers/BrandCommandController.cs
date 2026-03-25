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
        public async Task<ActionResult<ApiResponse<BrandDto>>> Create([FromBody] CreateBrandCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = product.Id },
                    ApiResponse<BrandDto>.Success(product, path, "Brand created successfully!")
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
        public async Task<ActionResult<ApiResponse<BrandDto>>> Update(int id, [FromBody] UpdateBrandCommand command)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command, id);

                if (product == null)
                {
                    return NotFound(ApiResponse<BrandDto>.Error(404, $"Brand with Id {id} not found!", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<BrandDto>.Success(product, path, "Brand updated successfully!"));
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