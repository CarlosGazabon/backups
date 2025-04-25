namespace inventio.Models.DTO.Supervisor
{
    public class DTOMetricsPerSupervisor
    {
        public DTOSupervisorEffContainer Efficiency { get; set; } = default!;
        public DTOSupervisorProductionContainer Production { get; set; } = default!;
        public DTOSupervisorScrapContainer Scrap { get; set; } = default!;
        public DTOSupervisorHoursContainer Hours { get; set; } = default!;
    }
}