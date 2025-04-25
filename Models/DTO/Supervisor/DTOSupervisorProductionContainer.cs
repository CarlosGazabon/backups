namespace inventio.Models.DTO.Supervisor
{
    public class DTOSupervisorProductionContainer
    {
        public List<DTOSupervisorProduction> ProductionPerSupervisor { get; set; } = default!;
        public decimal TotalProduction { get; set; }
    }
}