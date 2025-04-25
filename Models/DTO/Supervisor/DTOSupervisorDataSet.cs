namespace inventio.Models.DTO.Supervisor
{
    public class DTOSupervisorDataSet
    {
        public string Supervisor { get; set; } = default!;
        public decimal? Production { get; set; }
        public int? ScrapUnits { get; set; }
        public decimal? ScrapDen { get; set; }

    }
}