namespace inventio.Models.DTO.ProductivitySummary
{
    public class DTOScrapPerTime
    {
        public string Time { get; set; } = default!;
        public string Line { get; set; } = default!;
        public decimal PreformScrapPercentage { get; set; }
        public decimal BottleScrapPercentage { get; set; }
        public decimal CanScrapPercentage { get; set; }
        public decimal PouchScrapPercentage { get; set; }
        public decimal TotalScrapPercentage { get; set; }
    }
}