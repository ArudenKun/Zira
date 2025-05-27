namespace Domain.Entities.Users;

public class User
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; }
}
