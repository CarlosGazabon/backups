namespace inventio.Models.DTO.StatisticalChangeOver
{
    public class DTOStatisticalChangeOverFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<string> Lines { get; set; } = default!;
        public List<string> Shifts { get; set; } = default!;
        public List<string> Supervisors { get; set; } = default!;
        public List<string> SubCategories { get; set; } = default!;
        public List<string> Codes { get; set; } = default!;
    }
}