using System.ComponentModel.DataAnnotations;

namespace RoadSuite.Web.ViewModels;

public class ModerationDecisionViewModel
{
    public Guid CarId { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
