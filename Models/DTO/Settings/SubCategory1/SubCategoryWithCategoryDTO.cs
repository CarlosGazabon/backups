namespace inventio.Models.DTO.Settings.SubCategory1
{
    public class SubCategoryWithCategoryDTO
    {
        public int SubCategory1Id { get; set; }
        public int CategoryId { get; set; }
        public string SubCategory1 { get; set; } = default!;
        public string Category { get; set; } = default!;
        public bool Inactive { get; set; }
    }
}