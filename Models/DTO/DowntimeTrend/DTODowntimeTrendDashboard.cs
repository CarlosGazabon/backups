namespace inventio.Models.DTO.DowntimeTrend
{
    public class DTODowntimeTrendDashboard
    {
        public decimal TotalTableMinutes { get; set; }
        public List<DTOTrendTable> TrendTable { get; set; } = default!;
        public List<DTOTrendColumnChart> TrendColumn { get; set; } = default!;
    }
}