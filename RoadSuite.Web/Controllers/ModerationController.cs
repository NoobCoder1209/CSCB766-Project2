using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadSuite.Web.Data;
using RoadSuite.Web.Models;
using RoadSuite.Web.Services;
using RoadSuite.Web.ViewModels;

namespace RoadSuite.Web.Controllers;

[Authorize(Roles = "Moderator,Admin")]
public class ModerationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public ModerationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var pendingCars = await _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .Where(c => !c.IsDeleted && c.Status == CarStatus.Pending)
            .OrderBy(c => c.CreatedUtc)
            .AsNoTracking()
            .ToListAsync();

        return View(pendingCars);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var car = await _context.Cars
            .Include(c => c.Category)
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .Include(c => c.ModerationFeedback)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound();
        }

        return View(car);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        var car = await _context.Cars
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound();
        }

        car.Status = CarStatus.Approved;
        car.UpdatedUtc = DateTime.UtcNow;
        car.IsDeleted = false;
        car.DeletedUtc = null;

        await _context.SaveChangesAsync();

        if (car.DealerProfile?.UserId is { } dealerId)
        {
            await _notificationService.CreateAsync(dealerId, $"Your car {car.Make} {car.Model} has been approved.", car.Id);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Reject(Guid id)
    {
        var car = await _context.Cars
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound();
        }

        var model = new ModerationDecisionViewModel
        {
            CarId = car.Id
        };

        ViewBag.Car = car;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, ModerationDecisionViewModel model)
    {
        if (id != model.CarId)
        {
            return NotFound();
        }

        var car = await _context.Cars
            .Include(c => c.DealerProfile)!
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Car = car;
            return View(model);
        }

        car.Status = CarStatus.Rejected;
        car.UpdatedUtc = DateTime.UtcNow;

        var moderatorId = _userManager.GetUserId(User)!;
        _context.ModerationFeedback.Add(new ModerationFeedback
        {
            Id = Guid.NewGuid(),
            CarId = car.Id,
            ModeratorId = moderatorId,
            Reason = model.Reason,
            CreatedUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        if (car.DealerProfile?.UserId is { } dealerId)
        {
            await _notificationService.CreateAsync(dealerId, $"Your car {car.Make} {car.Model} was rejected: {model.Reason}", car.Id);
        }

        return RedirectToAction(nameof(Index));
    }
}
