using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoadSuite.Web.Data;
using RoadSuite.Web.Models;
using RoadSuite.Web.Services;
using RoadSuite.Web.ViewModels;

namespace RoadSuite.Web.Controllers;

[Authorize]
public class CarsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public CarsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] CarFilterViewModel filter)
    {
        var query = BuildFilteredQuery(filter, includeSensitive: User.IsInRole("Moderator") || User.IsInRole("Admin"));

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

        await PopulateFilterOptionsAsync(filter);

        ViewBag.SortOrder = filter.SortOrder;

        return View((paged, filter));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var car = await _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (car == null)
        {
            return NotFound();
        }

        if (car.Status != CarStatus.Approved && !CanViewNonPublicCars(car))
        {
            return Forbid();
        }

        return View(car);
    }

    [Authorize(Roles = "Dealer,Moderator,Admin")]
    public async Task<IActionResult> My([FromQuery] CarFilterViewModel filter)
    {
        var userId = _userManager.GetUserId(User)!;
        var query = _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)
            .Where(c => c.DealerProfile != null && c.DealerProfile.UserId == userId);

        ApplyFilter(ref query, filter, restrictToApproved: false);

        query = ApplySorting(query, filter.SortOrder);

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

        await PopulateFilterOptionsAsync(filter);
        ViewBag.SortOrder = filter.SortOrder;

        return View("My", (paged, filter));
    }

    [Authorize(Roles = "Dealer,Moderator,Admin")]
    public async Task<IActionResult> Create()
    {
        var model = new CarFormViewModel
        {
            Year = DateTime.UtcNow.Year,
            CategoryOptions = await GetCategorySelectListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Dealer,Moderator,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await GetCategorySelectListAsync();
            return View(model);
        }

        var dealerProfileId = await GetDealerProfileIdAsync();
        if (dealerProfileId == null)
        {
            ModelState.AddModelError(string.Empty, "Dealer profile not found. Contact administrator.");
            model.CategoryOptions = await GetCategorySelectListAsync();
            return View(model);
        }

        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = model.Make,
            Model = model.Model,
            Year = model.Year,
            Price = model.Price,
            Description = model.Description,
            CategoryId = model.CategoryId!.Value,
            DealerProfileId = dealerProfileId,
            Status = User.IsInRole("Moderator") || User.IsInRole("Admin") ? CarStatus.Approved : CarStatus.Pending,
            CreatedUtc = DateTime.UtcNow
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Dealer,Moderator,Admin")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var car = await GetEditableCarAsync(id, allowModerators: true);
        if (car == null)
        {
            return NotFound();
        }

        var model = new CarFormViewModel
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CategoryId = car.CategoryId,
            CategoryOptions = await GetCategorySelectListAsync(),
            ExistingStatus = car.Status.ToString()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Dealer,Moderator,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CarFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var car = await GetEditableCarAsync(id, allowModerators: true);
        if (car == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await GetCategorySelectListAsync();
            return View(model);
        }

        car.Make = model.Make;
        car.Model = model.Model;
        car.Year = model.Year;
        car.Price = model.Price;
        car.Description = model.Description;
        car.CategoryId = model.CategoryId!.Value;
        car.UpdatedUtc = DateTime.UtcNow;

        if (User.IsInRole("Dealer") && !User.IsInRole("Moderator") && !User.IsInRole("Admin"))
        {
            car.Status = CarStatus.Pending;
        }

        await _context.SaveChangesAsync();

        if (car.DealerProfile?.UserId is { } dealerId)
        {
            var statusMessage = car.Status == CarStatus.Pending ? "and is awaiting approval" : "";
            await _notificationService.CreateAsync(dealerId, $"Your car {car.Make} {car.Model} was updated {statusMessage}".Trim(), car.Id);
        }

        return RedirectToAction(nameof(My));
    }

    [Authorize(Roles = "Dealer,Moderator,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var car = await GetEditableCarAsync(id, allowModerators: true, allowAdmins: true);
        if (car == null)
        {
            return NotFound();
        }

        return View(car);
    }

    [HttpPost]
    [Authorize(Roles = "Dealer,Moderator,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, string deleteAction)
    {
        var allowModerators = User.IsInRole("Moderator") || User.IsInRole("Admin");
        var car = await GetEditableCarAsync(id, allowModerators, allowAdmins: allowModerators);
        if (car == null)
        {
            return NotFound();
        }

        var dealerIdForNotification = car.DealerProfile?.UserId;

        if (deleteAction == "mark")
        {
            car.Status = CarStatus.MarkedForDeletion;
            car.IsDeleted = true;
            car.DeletedUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else if (deleteAction == "permanent")
        {
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Unknown delete action.");
            return View(car);
        }

        if (dealerIdForNotification is { } dealerId)
        {
            await _notificationService.CreateAsync(dealerId, $"Your car {car.Make} {car.Model} was removed.", car.Id);
        }

        return RedirectToAction(nameof(My));
    }

    private IQueryable<Car> BuildFilteredQuery(CarFilterViewModel filter, bool includeSensitive)
    {
        var query = _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .Where(c => !c.IsDeleted);

        var restrictToApproved = !includeSensitive && !(User.Identity?.IsAuthenticated ?? false);

        ApplyFilter(ref query, filter, restrictToApproved);

        query = ApplySorting(query, filter.SortOrder);

        if (!includeSensitive && (User.Identity?.IsAuthenticated ?? false))
        {
            var userId = _userManager.GetUserId(User);
            if (User.IsInRole("Dealer") && !User.IsInRole("Moderator") && !User.IsInRole("Admin"))
            {
                query = query.Where(c => c.Status == CarStatus.Approved || (c.DealerProfile != null && c.DealerProfile.UserId == userId));
            }
            else
            {
                query = query.Where(c => c.Status == CarStatus.Approved);
            }
        }
        else if (restrictToApproved)
        {
            query = query.Where(c => c.Status == CarStatus.Approved);
        }

        return query;
    }

    private void ApplyFilter(ref IQueryable<Car> query, CarFilterViewModel filter, bool restrictToApproved)
    {
        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            var make = filter.Make.Trim();
            query = query.Where(c => c.Make.Contains(make));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }
        else if (restrictToApproved)
        {
            query = query.Where(c => c.Status == CarStatus.Approved);
        }
    }

    public static IQueryable<Car> ApplySorting(IQueryable<Car> query, string? sortOrder)
    {
        return sortOrder switch
        {
            "make" => query.OrderBy(c => c.Make).ThenBy(c => c.Model),
            "make_desc" => query.OrderByDescending(c => c.Make).ThenByDescending(c => c.Model),
            "price_desc" => query.OrderByDescending(c => c.Price),
            "price" => query.OrderBy(c => c.Price),
            "year_desc" => query.OrderByDescending(c => c.Year),
            "year" => query.OrderBy(c => c.Year),
            "created" => query.OrderBy(c => c.CreatedUtc),
            "created_desc" or _ => query.OrderByDescending(c => c.CreatedUtc)
        };
    }

    private async Task PopulateFilterOptionsAsync(CarFilterViewModel filter)
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        filter.CategoryOptions = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())
        {
            Selected = filter.CategoryId == c.Id
        });

        filter.StatusOptions = Enum.GetValues<CarStatus>()
            .Select(status => new SelectListItem(status.ToString(), status.ToString())
            {
                Selected = filter.Status == status
            })
            .ToList();
    }

    private bool CanViewNonPublicCars(Car car)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        if (User.IsInRole("Admin") || User.IsInRole("Moderator"))
        {
            return true;
        }

        if (User.IsInRole("Dealer") && car.DealerProfile?.UserId == _userManager.GetUserId(User))
        {
            return true;
        }

        return false;
    }

    private async Task<Car?> GetEditableCarAsync(Guid id, bool allowModerators = false, bool allowAdmins = true)
    {
        var car = await _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return null;
        }

        if (allowAdmins && User.IsInRole("Admin"))
        {
            return car;
        }

        if (allowModerators && User.IsInRole("Moderator"))
        {
            return car;
        }

        if (User.IsInRole("Dealer") && car.DealerProfile?.UserId == _userManager.GetUserId(User))
        {
            return car;
        }

        return null;
    }

    private async Task<Guid?> GetDealerProfileIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return null;
        }

        var profile = await _context.DealerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            return null;
        }

        return profile.Id;
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }
}
