using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrandService.Controllers
{
    [Route("api/brands")]
    [ApiController]
    public class BrandController : ControllerBase

    {
        private readonly IService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BrandDto>>>> GetAll()
        {
            try
            {
                var brands = await _brandService.GetAllAsync();

                return Ok(new ApiResponse<IEnumerable<BrandDto>>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách thương hiệu thành công",
                    Data = brands
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");

                return Ok(new ApiResponse<IEnumerable<BrandDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy danh sách thương hiệu",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(int id)
        {
            try
            {
                var brand = await _brandService.GetByIdAsync(id);

                if (brand == null)
                {
                    return Ok(new ApiResponse<BrandDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy thương hiệu với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin thương hiệu thành công",
                    Data = brand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brand {Id}", id);

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy thông tin thương hiệu",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Create(CreateBrandDto dto)
        {
            try
            {
                var brand = await _brandService.CreateAsync(dto);

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 201,
                    Message = "Tạo thương hiệu thành công",
                    Data = brand
                });
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 409,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi tạo thương hiệu",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<BrandDto>>> Update(int id, UpdateBrandDto dto)
        {
            try
            {
                var brand = await _brandService.UpdateAsync(id, dto);

                if (brand == null)
                {
                    return Ok(new ApiResponse<BrandDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy thương hiệu với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 200,
                    Message = "Cập nhật thương hiệu thành công",
                    Data = brand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {Id}", id);

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi cập nhật thương hiệu",
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
                var deleted = await _brandService.DeleteAsync(id);

                if (!deleted)
                {
                    return Ok(new ApiResponse
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy thương hiệu với ID {id}"
                    });
                }

                return Ok(new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Xóa thương hiệu thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {Id}", id);

                return Ok(new ApiResponse
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi xóa thương hiệu"
                });
            }
        }
    }
}