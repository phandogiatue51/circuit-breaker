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
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromForm] CreateProductCommand command, IFormFile? imageFile)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command, imageFile);

                return CreatedAtAction(
                    nameof(Create),
                    new { id = product.Id },
                    ApiResponse<ProductDto>.Success(product, path, "Product created successfully!")
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
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Error creating product", path, "INTERNAL_ERROR"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromForm] UpdateProductCommand command, IFormFile? imageFile)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var product = await _commandHandler.Handle(command, id, imageFile);

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductDto>.Error(404, $"Product with Id {id} not found", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse<ProductDto>.Success(product, path, "Product updated successfully"));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ApiResponse<ProductDto>.Error(ex.StatusCode, ex.Message, path, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, ApiResponse<ProductDto>.Error(500, "Error updating product", path, "INTERNAL_ERROR"));
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
                    return NotFound(ApiResponse<ProductDto>.Error(404, $"Product with Id {id} not found", path, "NOT_FOUND"));
                }

                return Ok(ApiResponse.Success(path, "Product deleted successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, ApiResponse.Error(500, "Error deleting product", path));
            }
        }
    }
}