namespace InfinityPOS.API.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = null!;
    public string RoleName { get; set; } = null!;
}