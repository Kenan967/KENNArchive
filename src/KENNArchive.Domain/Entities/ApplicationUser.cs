using Microsoft.AspNetCore.Identity;
namespace KENNArchive.Domain.Entities;
public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}