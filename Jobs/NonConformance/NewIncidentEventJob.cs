using Hangfire;
using Inventio.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Jobs.NonConformance;

public class NewIncidentEventJob
{
    private readonly ILogger<NewIncidentEventJob> _logger;

    private readonly ApplicationDBContext _context;

    public NewIncidentEventJob(ILogger<NewIncidentEventJob> logger, ApplicationDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task ExecuteAsync(int incidentId)
    {
        _logger.LogInformation("New Incident -> incidentId: {string}", incidentId.ToString());
        _logger.LogInformation("Sample job executed at: {time}", DateTimeOffset.Now);

        var inventory = await _context.NotificationInventory
            .FirstOrDefaultAsync(l => l.EventName == "non-conformance-new");

        var roles = inventory?.Roles?.Split(',') ?? Array.Empty<string>();

        var usersInRoles = await _context.UserRoles
            .Where(u => roles.Contains(u.RoleId))
            .ToListAsync();

        foreach (var userRole in usersInRoles)
        {
            // Console.WriteLine($"Enqueue<NotificationMessageJob> = UserId: {userRole.UserId}, RoleId: {userRole.RoleId}");
            BackgroundJob.Enqueue<NotificationMessageJob>(job => job.ExecuteAsync(incidentId, userRole.UserId, TemplateType.NonConformanceNew));
        }
    }
}