namespace inventio.Models.DTO.LinePerformance
{
    public class DTOSummary
    {
        public decimal Production { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Downtime { get; set; }
        public decimal Scrap { get; set; }
        public decimal Utilization { get; set; }
        public decimal ChangeOver { get; set; }
    }
}