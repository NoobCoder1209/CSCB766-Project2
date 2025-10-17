namespace RoadSuite.Web.Models;

public class Notification
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public Guid? CarId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    public ApplicationUser? User { get; set; }
    public Car? Car { get; set; }
}
