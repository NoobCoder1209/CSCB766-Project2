using Microsoft.AspNetCore.Identity;

namespace RoadSuite.Web.Models;

public class ApplicationUser : IdentityUser
{
    public DealerProfile? DealerProfile { get; set; }
}
