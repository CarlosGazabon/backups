namespace inventio.Models.DTO.DowntimeSku
{
    public class DTODowntimeSkuPerMinutes
    {
        public decimal Minutes { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public string Flavour { get; set; } = default!;
        public string NetContent { get; set; } = default!;
        public string Packing { get; set; } = default!;

    }
}