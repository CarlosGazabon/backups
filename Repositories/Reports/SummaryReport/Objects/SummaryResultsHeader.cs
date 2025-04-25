namespace inventio.Repositories.Reports.SummaryReport.Objects
{
    public class SummaryResultsHeader
    {
        public string Line { get; set; } = default!;
        public string Flow { get; set; } = default!;
        public string Production { get; set; } = default!;
        public string ProductionHrs { get; set; } = default!;
        public string DowntimeHrs { get; set; } = default!;
        public string ChangeOverHrs { get; set; } = default!;
    }
}