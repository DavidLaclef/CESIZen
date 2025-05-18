using Microsoft.AspNetCore.Identity;

namespace CESIZen.Data.Entities;

public class User : IdentityUser<int>
{
    // Ces propriétés sont déjà dans IdentityUser : UserName, Email, PasswordHash
    // Donc ne les redéfinit pas

    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Pseudo { get; set; } = null!;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsAccountActivated { get; set; } = true;
    public ICollection<RelaxingActivity>? FavoriteActivities { get; set; }
}
