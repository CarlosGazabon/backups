
namespace inventio.Models.DTO.Menu
{
    public class DTOSubMenuRequest
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Route { get; set; } = default!;
        public string Parent_id { get; set; } = default!;
        public int? IconId { get; set; }
        public required int Sort { get; set; }
    }
}