using System.ComponentModel.DataAnnotations;

namespace RoadSuite.Web.Models;

public class Category
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
