namespace RoadSuite.Web.Models;

public class DealerProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string DealershipName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }
    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
