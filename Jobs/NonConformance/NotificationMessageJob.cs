using Inventio.Data;
using Inventio.Models.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Jobs.NonConformance;

public partial class NotificationMessageJob
{
    private readonly ILogger<NotificationMessageJob> _logger;

    private readonly ApplicationDBContext _context;

    public NotificationMessageJob(ILogger<NotificationMessageJob> logger, ApplicationDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task ExecuteAsync(int incidentId, string messageToId, TemplateType templateType)
    {
        _logger.LogInformation("Job with incidentId: {string}", incidentId.ToString());
        _logger.LogInformation("Job executed at: {time}", DateTimeOffset.Now);

        var qualityIncident = await _context.QualityIncident
            .Include(q => q.AuditCreatedBy)
            .Include(q => q.Line)
            .Include(q => q.Product)
            .Include(q => q.QualityIncidentReason)
            .FirstOrDefaultAsync(q => q.Id == incidentId);

        var templateTypeString = templateType switch
        {
            TemplateType.NonConformanceNew => "non-conformance-new",
            TemplateType.NonConformanceEdit => "non-conformance-edit",
            TemplateType.NonConformanceRelease => "non-conformance-release",
            TemplateType.NonConformanceWeeklyHolds => "non-conformance-weekly-holds",
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };

        var notificationTemplate = await _context.NotificationTemplate
            .FirstOrDefaultAsync(nt => nt.Name == templateTypeString);

        var subject = notificationTemplate?.Subject ?? "Incident Subject";
        var body = notificationTemplate?.Body ?? "Incident Message Body";

        string lineName = qualityIncident?.Line?.Name ?? "Unknown Line";
        int cases = qualityIncident?.Quantity ?? 0;

        // Incident ID: {0}
        string incidentNumber = qualityIncident?.IncidentNumber ?? "Unknown Incident Number";

        // Date: {1}
        string dateOfIncident = qualityIncident?.DateOfIncident.ToString("yyyy-MM-dd") ?? "Unknown Date";

        // Issued by: {2}
        string auditCreatedBy = qualityIncident?.AuditCreatedBy?.UserName ?? "Unknown User";

        // Line: {3}
        string line = qualityIncident?.Line?.Name ?? "Unknown Line";

        // Product: {4}
        string product = qualityIncident?.Product?.Flavour ?? "Unknown Product";

        // Affected Cases: {5}
        int affectedCases = qualityIncident?.Quantity ?? 0;

        // Hold Reason: {6}
        string holdReason = qualityIncident?.QualityIncidentReason?.Description ?? "Unknown Hold Reason";

        // Next Steps: {7}
        string commentsNextSteps = qualityIncident?.ActionComments ?? "Unknown Next Steps";

        // Potential Root Cause: {8}
        string commentsRootCause = qualityIncident?.PotencialRootCause ?? "Unknown Root Cause";

        // Comments: {9}
        string comments = qualityIncident?.Comments ?? "Unknown Comments";

        // Unresolved Quality Incidents - OPEN Incidents = {0} - Cases Affected = {1}
        var holds = await _context.QualityIncident.Where(x => x.Released == false).ToListAsync();
        string holdCount = holds.Count().ToString();
        int holdCases = holds.Sum(x => x.Quantity);


        string subjectInterpolated = templateType switch
        {
            TemplateType.NonConformanceNew => string.Format(subject, lineName, cases),
            TemplateType.NonConformanceEdit => string.Format(subject, incidentNumber),
            TemplateType.NonConformanceRelease => string.Format(subject, lineName, cases),
            TemplateType.NonConformanceWeeklyHolds => string.Format(subject, holdCount, holdCases),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };

        string bodyInterpolated = templateType switch
        {
            TemplateType.NonConformanceNew => string.Format(body, incidentNumber, dateOfIncident, auditCreatedBy, line, product, affectedCases, holdReason, commentsNextSteps, commentsRootCause, comments, holdCount, holdCases),
            TemplateType.NonConformanceEdit => string.Format(body, incidentNumber, dateOfIncident, auditCreatedBy, line, product, affectedCases, holdReason, commentsNextSteps, commentsRootCause, comments, holdCount, holdCases),
            TemplateType.NonConformanceRelease => string.Format(body, incidentNumber, dateOfIncident, auditCreatedBy, line, product, affectedCases, holdReason, commentsNextSteps, commentsRootCause, comments, holdCount, holdCases),
            TemplateType.NonConformanceWeeklyHolds => string.Format(body, holdCount, holdCases),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };

        _context.NotificationMessage.Add(new NotificationMessage
        {
            SentTime = DateTime.Now,
            Subject = subjectInterpolated,
            Body = bodyInterpolated,
            Status = NotificationStatus.New,
            MessageToId = messageToId,
        });

        var result = await _context.SaveChangesAsync();

        _logger.LogInformation(result.ToString());
    }
}
