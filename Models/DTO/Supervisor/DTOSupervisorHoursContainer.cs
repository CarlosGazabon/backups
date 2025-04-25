namespace inventio.Models.DTO.Supervisor
{
    public class DTOSupervisorHoursContainer
    {
        public List<DTOSupervisorHours> HoursPerSupervisor { get; set; } = default!;
        public decimal TotalHours { get; set; }
    }
}