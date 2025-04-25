
namespace inventio.Models.DTO.DowntimeTrend
{
    public class DTOTrendTable
    {
        public string Date { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Line { get; set; } = default!;
        public string SubCategory2 { get; set; } = default!;
        public string Failure { get; set; } = default!;
        public decimal Minutes { get; set; } = default!;
    }
}