using inventio.Models.DTO.ProductivitySummary;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTOScrapSet
    {
        public List<DTOScrapPerLine>? ScrapPerLine { get; set; }
        public decimal TotalScrapPercent { get; set; }
    }
}