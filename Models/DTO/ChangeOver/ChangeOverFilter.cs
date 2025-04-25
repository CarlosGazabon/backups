namespace inventio.Models.DTO.ChangeOver
{
    public class ChangeOverFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<string>? Lines { get; set; } = default!;
        public List<string>? Shifts { get; set; } = default!;
        public List<string>? Supervisors { get; set; } = default!;
        public List<string>? SubCategory2 { get; set; } = default!;
    }
}