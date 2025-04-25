namespace inventio.Models.DTO.Supervisor
{
    public class SupervisorFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<int> Lines { get; set; } = default!;
        public List<int> Supervisors { get; set; } = default!;
        public List<int> Shifts { get; set; } = default!;
        public List<int> Categories { get; set; } = default!;
    }
}