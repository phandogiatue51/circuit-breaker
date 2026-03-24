namespace DTOs
{
    public enum AccountRole
    {
        Customer,
        Admin
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public AccountRole Role { get; set; }
        public string Email { get; set; } = string.Empty; 
        public int AccountId { get; set; }
    }
}
