using AuthService.Queries;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/queries/auth")]
    [Authorize] // Chỉ authenticated user mới được query
    public class AuthQueryController : ControllerBase
    {
        private readonly AuthQueryHandler _queryHandler;
        private readonly EventStoreService _eventStoreService;
        private readonly ILogger<AuthQueryController> _logger;

        public AuthQueryController(AuthQueryHandler queryHandler, ILogger<AuthQueryController> logger, EventStoreService eventStoreService)
        {
            _queryHandler = queryHandler;
            _logger = logger;
            _eventStoreService = eventStoreService;
        }

        /// <summary>
        /// QUERY: Lấy account theo id
        /// GET /api/queries/auth/account/{id}
        /// </summary>
        [HttpGet("account/{id}")]
        public async Task<ActionResult<ApiResponse<Account>>> GetAccountById(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetAccountByIdQuery { Id = id };
                var account = await _queryHandler.Handle(query);

                if (account == null)
                {
                    return NotFound(ApiResponse<Account>.Error(
                        404,
                        $"Account with Id {id} not found!",
                        path, "NOT_FOUND"
                    ));
                }

                // Không trả về password hash
                account.PasswordHash = string.Empty;

                return Ok(ApiResponse<Account>.Success(
                    account,
                    path,
                    "Account retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get account by id failed for id: {Id}", id);

                return StatusCode(500, ApiResponse<Account>.Error(
                    500,
                    "Error when retrieving account",
                    path, "INTERNAL_ERROR"
                ));
            }
        }

        /// <summary>
        /// QUERY: Lấy account theo email
        /// GET /api/queries/auth/account/email/{email}
        /// </summary>
        [HttpGet("account/email/{email}")]
        public async Task<ActionResult<ApiResponse<Account>>> GetAccountByEmail(string email)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new GetAccountByEmailQuery { Email = email };
                var account = await _queryHandler.Handle(query);

                if (account == null)
                {
                    return NotFound(ApiResponse<Account>.Error(
                        404,
                        $"Account with email {email} not found!",
                        path, "NOT_FOUND"
                    ));
                }

                // Không trả về password hash
                account.PasswordHash = string.Empty;

                return Ok(ApiResponse<Account>.Success(
                    account,
                    path,
                    "Account retrieved successfully!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get account by email failed for email: {Email}", email);

                return StatusCode(500, ApiResponse<Account>.Error(
                    500,
                    "Error when retrieving account",
                    path, "INTERNAL_ERROR"
                ));
            }
        }

        /// <summary>
        /// QUERY: Kiểm tra email đã tồn tại
        /// GET /api/queries/auth/check-email/{email}
        /// </summary>
        [HttpGet("check-email/{email}")]
        [AllowAnonymous] // Public endpoint để kiểm tra email khi register
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailExists(string email)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var query = new CheckEmailExistsQuery { Email = email };
                var exists = await _queryHandler.Handle(query);

                return Ok(ApiResponse<bool>.Success(
                    exists,
                    path,
                    exists ? "Email has existed" : "Email can be used"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check email exists failed for email: {Email}", email);

                return StatusCode(500, ApiResponse<bool>.Error(
                    500,
                    "Error when checking mail",
                    path, "INTERNAL_ERROR"
                ));
            }
        }

        [HttpGet("{id}/events")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<AuthEvent>>>> GetEvents(int id)
        {
            var path = HttpContext.Request.Path.ToString();

            try
            {
                var events = await _eventStoreService.GetEventsAsync(id);

                return Ok(ApiResponse<List<AuthEvent>>.Success(
                    events,
                    path,
                    events.Any() ? "Account history retrieved successfully!" : "Account history is empty"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for auth {Id}", id);
                return StatusCode(500, ApiResponse<List<AuthEvent>>.Error(500, "Error when retrieving account history", path, "INTERNAL_ERROR"));
            }
        }
    }
}