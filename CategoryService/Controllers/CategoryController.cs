using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryService.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase

    {
        private readonly IService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
        {
            try
            {
                var brands = await _categoryService.GetAllAsync();

                return Ok(new ApiResponse<IEnumerable<CategoryDto>>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách phân loại thành công",
                    Data = brands
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all brands");

                return Ok(new ApiResponse<IEnumerable<CategoryDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy danh sách phân loại",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
        {
            try
            {
                var brand = await _categoryService.GetByIdAsync(id);

                if (brand == null)
                {
                    return Ok(new ApiResponse<CategoryDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy phân loại với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin phân loại thành công",
                    Data = brand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {Id}", id);

                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy thông tin phân loại",
                    Data = null
                });
            }
        }

        [HttpGet("by-ids")]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetByIds([FromQuery] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return Ok(new ApiResponse<List<CategoryDto>>
                    {
                        StatusCode = 400,
                        Message = "Vui lòng cung cấp danh sách ID phân loại",
                        Data = null
                    });
                }

                var categories = await _categoryService.GetByIdsAsync(ids);

                return Ok(new ApiResponse<List<CategoryDto>>
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách phân loại thành công",
                    Data = categories.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories by ids: {Ids}", string.Join(",", ids));

                return Ok(new ApiResponse<List<CategoryDto>>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi lấy danh sách phân loại",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryDto dto)
        {
            try
            {
                var brand = await _categoryService.CreateAsync(dto);

                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 201,
                    Message = "Tạo phân loại thành công",
                    Data = brand
                });
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 409,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");

                return Ok(new ApiResponse<BrandDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi tạo phân loại",
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, UpdateCategoryDto dto)
        {
            try
            {
                var brand = await _categoryService.UpdateAsync(id, dto);

                if (brand == null)
                {
                    return Ok(new ApiResponse<CategoryDto>
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy phân loại với ID {id}",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 200,
                    Message = "Cập nhật phân loại thành công",
                    Data = brand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);

                return Ok(new ApiResponse<CategoryDto>
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi cập nhật phân loại",
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
                var deleted = await _categoryService.DeleteAsync(id);

                if (!deleted)
                {
                    return Ok(new ApiResponse
                    {
                        StatusCode = 404,
                        Message = $"Không tìm thấy phân loại với ID {id}"
                    });
                }

                return Ok(new ApiResponse
                {
                    StatusCode = 200,
                    Message = "Xóa phân loại thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);

                return Ok(new ApiResponse
                {
                    StatusCode = 500,
                    Message = "Có lỗi khi xóa phân loại"
                });
            }
        }
    }
}