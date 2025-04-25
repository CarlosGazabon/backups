namespace inventio.Models.DTO.Menu
{
    public class DTOSubMenu
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Route { get; set; } = default!;
        public string Parent_id { get; set; } = default!;
        public string? ParentLabel { get; set; }
        public int? IconId { get; set; }
        public string? IconName { get; set; } = default!;
        public int? Sort { get; set; }
    }
}