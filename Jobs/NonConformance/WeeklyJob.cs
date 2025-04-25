using Hangfire;
using Inventio.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Jobs.NonConformance;

public class WeeklyJob
{
    private readonly ILogger<WeeklyJob> _logger;

    private readonly ApplicationDBContext _context;

    public WeeklyJob(ILogger<WeeklyJob> logger, ApplicationDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Weekly Job executed at: {time}", DateTimeOffset.Now);

        var inventory = await _context.NotificationInventory
            .FirstOrDefaultAsync(l => l.EventName == "non-conformance-weekly-holds");

        var roles = inventory?.Roles?.Split(',') ?? Array.Empty<string>();

        var usersInRoles = await _context.UserRoles
            .Where(u => roles.Contains(u.RoleId))
            .ToListAsync();

        foreach (var userRole in usersInRoles)
        {
            BackgroundJob.Enqueue<NotificationMessageJob>(job => job.ExecuteAsync(0, userRole.UserId, TemplateType.NonConformanceWeeklyHolds));
        }
    }
}
