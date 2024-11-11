using Microsoft.AspNetCore.Identity;

namespace VstepPractice.API.Models.Entities;

public class Role : IdentityRole<int>
{
    public Role() : base() { }

    public Role(string roleName) : base(roleName) { }
}
