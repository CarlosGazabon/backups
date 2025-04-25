namespace inventio.Models.DTO.Settings.SubCategory2
{
    public class SubCategory2WithSubCategory1DTO
    {
        public int CategoryId { get; set; }
        public string Category { get; set; } = default!;
        public int SubCategory1Id { get; set; }
        public string SubCategory1 { get; set; } = default!;
        public int SubCategory2Id { get; set; }
        public string SubCategory2 { get; set; } = default!;
        public bool Inactive { get; set; }
    }
}