using System.ComponentModel.DataAnnotations;

namespace RoadSuite.Web.Models;

public class Car
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; }

    [Range(0, 1000000)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    public Guid? DealerProfileId { get; set; }

    public CarStatus Status { get; set; } = CarStatus.Pending;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedUtc { get; set; }

    public Category? Category { get; set; }

    public DealerProfile? DealerProfile { get; set; }

    public ICollection<ModerationFeedback> ModerationFeedback { get; set; } = new List<ModerationFeedback>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
