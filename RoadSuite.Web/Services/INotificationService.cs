using RoadSuite.Web.Models;

namespace RoadSuite.Web.Services;

public interface INotificationService
{
    Task CreateAsync(string userId, string message, Guid? carId = null, CancellationToken cancellationToken = default);
}
