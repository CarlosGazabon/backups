namespace inventio.Models.DTO.DowntimeSku
{
    public class DTODowntimeSkuTable
    {
        public decimal Minutes { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public string Line { get; set; } = default!;
    }
}