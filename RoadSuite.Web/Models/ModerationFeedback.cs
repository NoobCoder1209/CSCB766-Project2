namespace RoadSuite.Web.Models;

public class ModerationFeedback
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }
    public string ModeratorId { get; set; } = default!;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public Car? Car { get; set; }
}
