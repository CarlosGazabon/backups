namespace inventio.Models.DTO.ProductivitySummary
{
    public class DTOScrapTable
    {
        public string Line { get; set; } = default!;
        public decimal ScrapDEN { get; set; }
        public decimal PreformScrap { get; set; }
        public decimal PreformScrapPercentage { get; set; }
        public decimal BottleScrap { get; set; }
        public decimal BottleScrapPercentage { get; set; }
        public decimal CanScrap { get; set; }
        public decimal CanScrapPercentage { get; set; }
        public decimal PouchScrap { get; set; }
        public decimal PouchScrapPercentage { get; set; }
        public decimal TotalScrap { get; set; }
        public decimal TotalScrapPercentage { get; set; }
    }
}