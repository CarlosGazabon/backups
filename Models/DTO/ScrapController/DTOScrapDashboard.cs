namespace inventio.Models.DTO.ProductivitySummary
{
    public class DTOScrapDashboard
    {
        public decimal PlantScrapPercentage { get; set; } = default!;
        public List<DTOScrapPerLine> ScrapPerLine { get; set; } = default!;
        public List<DTOScrapTable> ScrapTable { get; set; } = default!;
        public List<DTOScrapPerTime> ScrapPerTime { get; set; } = default!;
    }
}