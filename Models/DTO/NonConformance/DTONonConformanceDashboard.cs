namespace inventio.Models.DTO.NonConformance
{
  public class DTONonConformanceDashboard
  {
    public class Summary
    {
      public int Incidents { get; set; }
      public int Cases { get; set; }
    }

    public class QuantityByReason
    {
      public required QualityIncidentReason QualityIncidentReason { get; set; }
      public int Cases { get; set; }
      public double Percentage { get; set; }
    }

    public class QualityIncidentReason
    {
      public int Id { get; set; }
      public required string Description { get; set; }
    }

    public class IncidentsByLine
    {
      public required Line Line { get; set; }
      public int Incidents { get; set; }
      public double Percentage { get; set; }
      public int Cases { get; set; }
    }

    public class QuantityByLine
    {
      public required Line Line { get; set; }
      public int Cases { get; set; }
      public double Percentage { get; set; }
    }

    public class Line
    {
      public int Id { get; set; }
      public required string Name { get; set; }
    }

    public class ReleasedVsNotReleased
    {
      public int Released { get; set; }
      public int NotReleased { get; set; }
      public double ReleasedPercentage { get; set; }
      public double NotReleasedPercentage { get; set; }
      public int ReleasedQuantity { get; set; }
      public int NotReleasedQuantity { get; set; }
      public double ReleasedQuantityPercentage { get; set; }
      public double NotReleasedQuantityPercentage { get; set; }
    }

    public class DaysOnHold
    {
      public int Id { get; set; }
      public required string IncidentNumber { get; set; }
      public int DaysNotReleased { get; set; }
    }

    public class IncidentsByMonth
    {
      public int Year { get; set; }
      public int Month { get; set; }
      public int Incidents { get; set; }
      public int Cases { get; set; }
    }

    public class MonthWithMostIncidents
    {
      public required int Year { get; set; }
      public required int Month { get; set; }
      public required int Incidents { get; set; }
      public required int Cases { get; set; }
    }

    public class MonthWithMostCases
    {
      public int Year { get; set; }
      public int Month { get; set; }
      public int Incidents { get; set; }
      public int Cases { get; set; }
    }


    public class QuantityByReleaseType
    {
      public required string ReleaseType { get; set; }
      public int Cases { get; set; }
      public double CasesPercentage { get; set; }
      public int Incidents { get; set; }
      public double IncidentsPercentage { get; set; }
    }

    public class ReleasedProductDetails
    {
      public required string ReleaseType { get; set; }
      public int Cases { get; set; }
      public double CasesPercentage { get; set; }
    }

    public class LongestOnHold
    {
      public required string IncidentNumber { get; set; }
      public int DaysOnHold { get; set; }
    }

    public required Summary summary { get; set; }
    public required List<QuantityByReleaseType> quantityByReleaseType { get; set; }
    public required List<ReleasedProductDetails> releasedProductDetails { get; set; }
    public required List<QuantityByReason> quantityByReason { get; set; }
    public required List<IncidentsByLine> incidentsByLine { get; set; }
    public required List<QuantityByLine> quantityByLine { get; set; }
    public required ReleasedVsNotReleased releasedVsNotReleased { get; set; }
    public required List<DaysOnHold> daysOnHold { get; set; }
    public required List<IncidentsByMonth> incidentsByMonth { get; set; }
    public required MonthWithMostIncidents monthWithMostIncidents { get; set; }
    public required MonthWithMostCases monthWithMostCases { get; set; }

    public required LongestOnHold longestOnHold { get; set; }

    public required double averageDaysToRelease { get; set; }
  }
}