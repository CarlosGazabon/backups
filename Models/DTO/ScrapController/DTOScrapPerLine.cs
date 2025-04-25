namespace inventio.Models.DTO.ProductivitySummary
{
    public class DTOScrapPerLine
    {
        public string Line { get; set; } = default!;
        public string ScrapType { get; set; } = default!;
        public decimal ScrapPercentage { get; set; }
    }
}