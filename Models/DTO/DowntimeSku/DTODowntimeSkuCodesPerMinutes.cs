namespace inventio.Models.DTO.DowntimeSku
{
    public class DTODowntimeSkuCodesPerMinutes
    {
        public decimal Minutes { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string SubCategory2 { get; set; } = default!;
        public string Failure { get; set; } = default!;
    }
}