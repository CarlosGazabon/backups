namespace inventio.Models.DTO.Supervisor
{
    public class DTOSupervisorEffContainer
    {
        public List<DTOSupervisorEff> EffPerSupervisor { get; set; } = default!;
        public decimal TotalEff { get; set; }
    }
}