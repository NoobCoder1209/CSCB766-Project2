using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RoadSuite.Web.ViewModels;

public class CarFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid? CategoryId { get; set; }

    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public string? ExistingStatus { get; set; }
}
