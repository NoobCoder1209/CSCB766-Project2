using Microsoft.AspNetCore.Mvc.Rendering;
using RoadSuite.Web.Models;

namespace RoadSuite.Web.ViewModels;

public class CarFilterViewModel
{
    public string? Make { get; set; }
    public Guid? CategoryId { get; set; }
    public CarStatus? Status { get; set; }
    public string? SortOrder { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> StatusOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> PageSizeOptions { get; set; } = new[] { 5, 10, 20 }
        .Select(size => new SelectListItem(size.ToString(), size.ToString()));
}
