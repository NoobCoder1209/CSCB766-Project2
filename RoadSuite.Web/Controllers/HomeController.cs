using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoadSuite.Web.Data;
using RoadSuite.Web.Models;
using RoadSuite.Web.ViewModels;

namespace RoadSuite.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] CarFilterViewModel filter)
    {
        var query = _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)
            .Where(c => !c.IsDeleted);

        var isModerator = User.IsInRole("Moderator") || User.IsInRole("Admin");
        var isDealer = User.IsInRole("Dealer") && !isModerator;
        var userId = _userManager.GetUserId(User);

        if (!isModerator)
        {
            if (isDealer && userId != null)
            {
                query = query.Where(c => c.Status == CarStatus.Approved || (c.DealerProfile != null && c.DealerProfile.UserId == userId));
            }
            else
            {
                query = query.Where(c => c.Status == CarStatus.Approved);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            var make = filter.Make.Trim();
            query = query.Where(c => c.Make.Contains(make));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
        }

        if (isModerator && filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        query = CarsController.ApplySorting(query, filter.SortOrder);

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var paged = new PagedResult<Car>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalItems = totalItems
        };

        filter.CategoryOptions = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString())
            {
                Selected = filter.CategoryId == c.Id
            })
            .ToListAsync();

        filter.StatusOptions = Enum.GetValues<CarStatus>()
            .Select(status => new SelectListItem(status.ToString(), status.ToString())
            {
                Selected = filter.Status == status
            })
            .ToList();

        if (!isModerator)
        {
            filter.StatusOptions = filter.StatusOptions.Select(option =>
            {
                option.Selected = option.Value == filter.Status?.ToString();
                return option;
            });
        }

        ViewBag.SortOrder = filter.SortOrder;

        return View((paged, filter));
    }
}
