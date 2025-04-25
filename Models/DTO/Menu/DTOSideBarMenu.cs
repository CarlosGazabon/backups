namespace inventio.Models.DTO.Menu
{
    public class DTOSideBarMenu
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string? Route { get; set; }
        public string? Parent_id { get; set; }
        public string? Icon { get; set; }
        public int? Sort { get; set; }
        public bool? Can_view { get; set; } = false;
        public bool? Can_add { get; set; } = false;
        public bool? Can_delete { get; set; } = false;
        public bool? Can_modify { get; set; } = false;
    }
}