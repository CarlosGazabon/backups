namespace inventio.Models.DTO.DowntimeSku
{
    public class DowntimeSkuFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<int> Shifts { get; set; } = default!;
        public List<int> Categories { get; set; } = default!;
        public List<int> Lines { get; set; } = default!;
        public List<string> Packing { get; set; } = default!;
        public List<int> Sku { get; set; } = default!;
    }
}