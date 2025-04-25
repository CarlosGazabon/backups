namespace inventio.Models.DTO.DowntimeXSubcat
{
    public class DTODowntimeFilters
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<int> Lines { get; set; } = default!;
        public List<int> Categories { get; set; } = default!;
        public List<int> Shifts { get; set; } = default!;
        public List<int> SubCategories { get; set; } = default!;
    }
}