namespace inventio.Models.DTO.DowntimeTrend
{
    public class DTOTrendFilter
    {
        public string Time { get; set; } = default!;
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<int> Lines { get; set; } = default!;
        public List<int> Categories { get; set; } = default!;
        public List<int> SubCategories { get; set; } = default!;
        public List<int> Codes { get; set; } = default!;
    }
}