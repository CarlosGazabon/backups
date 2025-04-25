namespace inventio.Models.DTO.DowntimeSku
{
    public class DTODowntimeSkuDashboard
    {
        public DTODowntimeSkuTotal TotalDowntime { get; set; } = default!;
        public List<DTODowntimeSkuTable> DowntimeSkuTables { get; set; } = default!;
        public List<DTODowntimeSkuPerMinutes> DowntimeSkuMinutes { get; set; } = default!;
        public List<DTODowntimeSkuCodesPerMinutes> DowntimePerSkuCodesMinutes { get; set; } = default!;
    }
}

