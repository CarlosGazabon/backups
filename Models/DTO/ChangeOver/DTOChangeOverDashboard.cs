namespace inventio.Models.DTO.ChangeOver
{
    public class DTOChangeOverDashboard
    {
        public DTOChangeOverMetrics ChangeOverMetrics { get; set; } = default!;
        public List<DTOChangeOverTable> ChangeOverTable { get; set; } = default!;
    }
}