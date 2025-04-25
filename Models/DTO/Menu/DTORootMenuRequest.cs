namespace inventio.Models.DTO.Menu
{
    public class DTORootMenuRequest
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public required string Route { get; set; } = "";
        public int IconId { get; set; }
        public int Sort { get; set; }
    }
}