
namespace inventio.Models.DTO.ChangeOver
{
    public class DTOChangeOverTable
    {
        public string SubCategory2 { get; set; } = default!;
        public string Line { get; set; } = default!;
        public decimal Minutes { get; set; }
        public decimal Quantity { get; set; }
    }
}