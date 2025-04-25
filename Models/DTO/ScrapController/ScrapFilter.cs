namespace inventio.Models.DTO.ProductivitySummary
{
    public class ScrapFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<string> Lines { get; set; } = default!;
        public string? Time { get; set; } = default!;
        public List<string>? Shifts { get; set; }
    }
}