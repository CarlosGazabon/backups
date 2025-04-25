namespace inventio.Models.DTO.StatisticalChangeOver
{
    public class DTOStatisticalChangeOverDashboard
    {
        public List<DTOTableChangeOver> TableChangeOver { get; set; } = default!;
        public List<DTOHistogram> Histogram { get; set; } = default!;
    }
}