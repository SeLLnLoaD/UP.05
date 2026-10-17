namespace DeliveryService.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    public int RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}