namespace AuthService.Queries
{

    public class AuthQueryHandler
    {
        private readonly Repository _repository;
        private readonly ILogger<AuthQueryHandler> _logger;

        public AuthQueryHandler(Repository repository, ILogger<AuthQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// QUERY: Lấy account theo email (chỉ đọc)
        /// </summary>
        public async Task<Account?> Handle(GetAccountByEmailQuery query)
        {
            _logger.LogInformation("Handling GetAccountByEmailQuery: {Email}", query.Email);
            return await _repository.GetByEmailAsync(query.Email);
        }

        /// <summary>
        /// QUERY: Lấy account theo id (chỉ đọc)
        /// </summary>
        public async Task<Account?> Handle(GetAccountByIdQuery query)
        {
            _logger.LogInformation("Handling GetAccountByIdQuery: {Id}", query.Id);
            return await _repository.GetByIdAsync(query.Id);
        }

        /// <summary>
        /// QUERY: Kiểm tra email đã tồn tại (chỉ đọc)
        /// </summary>
        public async Task<bool> Handle(CheckEmailExistsQuery query)
        {
            _logger.LogInformation("Handling CheckEmailExistsQuery: {Email}", query.Email);
            return await _repository.EmailExistsAsync(query.Email);
        }
    }
}