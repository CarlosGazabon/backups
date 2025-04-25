namespace inventio.Models.DTO.DowntimeTrend
{
    public class DTOTrendTableSet
    {
        public List<DTOTrendTable> TrendTable { get; set; } = default!;
        public decimal TotalMinutes { get; set; }
    }
}