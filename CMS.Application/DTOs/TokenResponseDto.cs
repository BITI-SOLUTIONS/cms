namespace CMS.Application.DTOs
{
    /// <summary>
    /// Response con el token generado por el API
    /// </summary>
    public class TokenResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string? Message { get; set; }
    }
}