namespace inventio.Models.DTO.Supervisor
{
    public class DTOSupervisorScrapContainer
    {
        public List<DTOSupervisorScrap> ScrapPerSupervisor { get; set; } = default!;
        public decimal TotalScrap { get; set; }
    }
}