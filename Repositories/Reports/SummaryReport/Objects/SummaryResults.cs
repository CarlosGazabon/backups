namespace inventio.Repositories.Reports.SummaryReport.Objects
{
    public class SummaryResults
    {
        public string Line { get; set; } = default!;
        public decimal? Production { get; set; }
        public decimal? EffDen { get; set; }
        public int Flow { get; set; }
        public decimal? ProductionHrs { get; set; }
        public decimal? DowntimeHrs { get; set; }
        public decimal? ChangeOverHrs { get; set; }
    }
}