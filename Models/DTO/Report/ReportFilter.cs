namespace inventio.Models.DTO.Report
{
    public class ReportFilter
    {
        public string date { get; set; } = default!;
        public int line { get; set; }
        public int shift { get; set; }
        public int supervisor { get; set; }
        public string name { get; set; } = default!;
        public string cat { get; set; } = default!;
    }
}