using System.Data;
using inventio.Models.DTO.NonConformance;
using Inventio.Data;
using Inventio.Models;
using Microsoft.EntityFrameworkCore;
using static inventio.Models.DTO.NonConformance.DTONonConformanceDashboard;

namespace inventio.Repositories.Dashboards.NonConformanceAnalysis
{
  public class NonConformanceAnalysisRepository : INonConformanceAnalysisRepository
  {
    private readonly ApplicationDBContext _context;

    public NonConformanceAnalysisRepository(ApplicationDBContext context)
    {
      _context = context;
    }


    //* --------------------------------- Filters -------------------------------- */
    public async Task<IEnumerable<Inventio.Models.Line>> GetLines(NonConformanceFilter filters)
    {
      DateOnly startDate = DateOnly.Parse(filters.StartDate);
      DateOnly endDate = DateOnly.Parse(filters.EndDate).AddDays(1);

      var lines = await _context.QualityIncident
      .Where(w => w.DateOfIncident >= startDate && w.DateOfIncident <= endDate)
      .Select(s => s.Line!)
      .Distinct()
      .ToListAsync();

      return lines;
    }

    //* -------------------------------- Dashboard ------------------------------- */
    public async Task<DTONonConformanceDashboard> GetDashboard(NonConformanceFilter filters)
    {
      DateOnly startDate = DateOnly.Parse(filters.StartDate);
      DateOnly endDate = DateOnly.Parse(filters.EndDate);

      var lineIds = filters.Lines?.Select(int.Parse).ToArray() ?? Array.Empty<int>();

      var incidents = await _context.QualityIncident
          .Where(q => q.DateOfIncident >= startDate && q.DateOfIncident <= endDate)
          .Where(q => lineIds.Contains(q.LineId))
          .ToListAsync();

      var totalIncidents = incidents.Count;
      var totalCases = incidents.Sum(q => q.Quantity);

      var releasedCount = incidents.Count(q => q.Released);
      var notReleasedCount = incidents.Count(q => !q.Released);
      var releasedQuantity = incidents.Where(q => q.Released).Sum(q => q.Quantity);
      var notReleasedQuantity = incidents.Where(q => !q.Released).Sum(q => q.Quantity);

      var releasedVsNotReleased = new DTONonConformanceDashboard.ReleasedVsNotReleased
      {
        Released = releasedCount,
        NotReleased = notReleasedCount,
        ReleasedPercentage = Math.Round((double)releasedCount / totalIncidents * 100, 2),
        NotReleasedPercentage = Math.Round((double)notReleasedCount / totalIncidents * 100, 2),
        ReleasedQuantity = releasedQuantity,
        NotReleasedQuantity = notReleasedQuantity,
        ReleasedQuantityPercentage = Math.Round((double)releasedQuantity / totalCases * 100, 2),
        NotReleasedQuantityPercentage = Math.Round((double)notReleasedQuantity / totalCases * 100, 2)
      };

      var notReleasedIncidents = await _context.QualityIncident
          .Where(q => !q.Released)
          .ToListAsync();

      var today = DateOnly.FromDateTime(DateTime.Now);
      var daysOnHold = notReleasedIncidents
          .Select(q => new DTONonConformanceDashboard.DaysOnHold
          {
            Id = q.Id,
            IncidentNumber = q.IncidentNumber,
            DaysNotReleased = (today.ToDateTime(TimeOnly.MinValue) - q.DateOfIncident.ToDateTime(TimeOnly.MinValue)).Days
          })
          .ToList();

      var incidentsByMonth = incidents
          .GroupBy(q => new { q.DateOfIncident.Year, q.DateOfIncident.Month })
          .Select(g => new DTONonConformanceDashboard.IncidentsByMonth
          {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Incidents = g.Count(),
            Cases = g.Sum(q => q.Quantity)
          })
          .OrderBy(g => g.Year)
          .ThenBy(g => g.Month)
          .ToList();

      var summary = new DTONonConformanceDashboard.Summary
      {
        Incidents = totalIncidents,
        Cases = totalCases
      };

      var quantityByReason = incidents
          .GroupBy(q => q.QualityIncidentReasonId)
          .Select(g => new DTONonConformanceDashboard.QuantityByReason
          {
            QualityIncidentReason = new DTONonConformanceDashboard.QualityIncidentReason
            {
              Id = g.Key,
              Description = _context.QualityIncidentReason
                  .Where(r => r.Id == g.Key)
                  .Select(r => r.Description)
                  .FirstOrDefault()!
            },
            Cases = g.Sum(q => q.Quantity),
            Percentage = Math.Round((double)g.Sum(q => q.Quantity) / totalCases * 100, 2)
          })
          .OrderByDescending(q => q.Cases)
          .ToList();

      var incidentsByLine = incidents
                .GroupBy(q => q.LineId)
                .Select(g => new DTONonConformanceDashboard.IncidentsByLine
                {
                  Line = _context.Line
                        .Where(l => l.Id == g.Key)
                        .Select(l => new DTONonConformanceDashboard.Line { Id = l.Id, Name = l.Name })
                        .FirstOrDefault()!,
                  Incidents = g.Count(),
                  Percentage = Math.Round((double)g.Count() / totalIncidents * 100, 2),
                  Cases = g.Sum(q => q.Quantity)
                })
                .OrderByDescending(q => q.Incidents)
                .ToList();

      var quantityByLine = incidents
                .GroupBy(q => q.LineId)
                .Select(g => new DTONonConformanceDashboard.QuantityByLine
                {
                  Line = _context.Line
                        .Where(l => l.Id == g.Key)
                        .Select(l => new DTONonConformanceDashboard.Line { Id = l.Id, Name = l.Name })
                        .FirstOrDefault()!,
                  Cases = g.Sum(q => q.Quantity),
                  Percentage = Math.Round((double)g.Sum(q => q.Quantity) / totalCases * 100, 2)
                })
                .OrderByDescending(q => q.Cases)
                .ToList();

      var monthWithMostIncidentsData = incidentsByMonth
                .OrderByDescending(g => g.Incidents)
                .FirstOrDefault();

      var monthWithMostIncidents = new DTONonConformanceDashboard.MonthWithMostIncidents
      {
        Year = monthWithMostIncidentsData.Year,
        Month = monthWithMostIncidentsData.Month,
        Incidents = monthWithMostIncidentsData.Incidents,
        Cases = monthWithMostIncidentsData.Cases
      };

      var monthWithMostCasesData = incidentsByMonth
          .OrderByDescending(g => g.Cases)
          .FirstOrDefault();

      var monthWithMostCases = new DTONonConformanceDashboard.MonthWithMostCases
      {
        Year = monthWithMostCasesData.Year,
        Month = monthWithMostCasesData.Month,
        Incidents = monthWithMostCasesData.Incidents,
        Cases = monthWithMostCasesData.Cases
      };


      var releasedIncidets = incidents.Where(q => q.Released).ToList();
      var onHoldIncidets = incidents.Where(q => !q.Released).ToList();


      double averageDaysToRelease = 0;
      if (releasedIncidets.Count > 0)
      {
        averageDaysToRelease = releasedIncidets
          .Average(q => q.DateOfRelease.HasValue ? (q.DateOfRelease.Value.ToDateTime(TimeOnly.MinValue) - q.DateOfIncident.ToDateTime(TimeOnly.MinValue)).TotalDays : 0);
        averageDaysToRelease = Math.Round(averageDaysToRelease, 2);
      }

      var releasedProductDetails = new List<ReleasedProductDetails>
      {
          new ReleasedProductDetails
          {
              ReleaseType = "Consumption",
              Cases = releasedIncidets.Sum(q => q.ReleasedForConsumption),
              CasesPercentage = Math.Round((double)releasedIncidets.Sum(q => q.ReleasedForConsumption) / totalCases * 100, 2),
          },
          new ReleasedProductDetails
          {
              ReleaseType = "Donation",
              Cases = releasedIncidets.Sum(q => q.ReleasedForDonation),
              CasesPercentage = Math.Round((double)releasedIncidets.Sum(q => q.ReleasedForDonation) / totalCases * 100, 2),
          },
          new ReleasedProductDetails
          {
            ReleaseType = "Destruction",
            Cases = releasedIncidets.Sum(q => q.ReleasedForDestruction),
            CasesPercentage = Math.Round((double)releasedIncidets.Sum(q => q.ReleasedForDestruction) / totalCases * 100, 2),
          },
          new ReleasedProductDetails
          {
            ReleaseType = "Other",
            Cases = releasedIncidets.Sum(q => q.ReleasedForOther),
            CasesPercentage = Math.Round((double)releasedIncidets.Sum(q => q.ReleasedForOther) / totalCases * 100, 2),
          }
      };

      var quantityByReleaseType = new List<QuantityByReleaseType>
      {
          new QuantityByReleaseType
          {
            ReleaseType = "ON-HOLD",
            Cases = onHoldIncidets.Sum(q => q.Quantity),
            CasesPercentage = Math.Round((double)onHoldIncidets.Sum(q => q.Quantity) / totalCases * 100, 2),
            Incidents = onHoldIncidets.Count(),
            IncidentsPercentage = Math.Round((double)onHoldIncidets.Count() / totalIncidents * 100, 2)
          }
          ,
          new QuantityByReleaseType
          {
            ReleaseType = "Released",
            Cases = releasedIncidets.Sum(q => q.Quantity),
            CasesPercentage = Math.Round((double)releasedIncidets.Sum(q => q.Quantity) / totalCases * 100, 2),
            Incidents = releasedIncidets.Count(),
            IncidentsPercentage = Math.Round((double)releasedIncidets.Count() / totalIncidents * 100, 2)
          }
      };

      var longestOnHoldIncident = onHoldIncidets
          .OrderByDescending(q => (DateTime.UtcNow - q.DateOfIncident.ToDateTime(TimeOnly.MinValue)).Days)
          .FirstOrDefault();

      var longestOnHoldConsecutive = longestOnHoldIncident?.IncidentNumber;
      var daysOnHoldCount = longestOnHoldIncident != null ? (DateTime.UtcNow - longestOnHoldIncident.DateOfIncident.ToDateTime(TimeOnly.MinValue)).Days : 0;

      var longestOnHold = new LongestOnHold
      {
        IncidentNumber = longestOnHoldConsecutive ?? string.Empty,
        DaysOnHold = daysOnHoldCount
      };

      DTONonConformanceDashboard nonConformanceDashboard = new()
      {
        summary = summary,
        quantityByReason = quantityByReason!,
        incidentsByLine = incidentsByLine!,
        quantityByLine = quantityByLine!,
        releasedVsNotReleased = releasedVsNotReleased!,
        daysOnHold = daysOnHold!,
        incidentsByMonth = incidentsByMonth!,
        monthWithMostIncidents = monthWithMostIncidents!,
        monthWithMostCases = monthWithMostCases!,
        quantityByReleaseType = quantityByReleaseType,
        releasedProductDetails = releasedProductDetails,
        longestOnHold = longestOnHold,
        averageDaysToRelease = averageDaysToRelease
      };

      return nonConformanceDashboard;
    }
  }
}
