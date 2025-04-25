namespace inventio.Models.DTO.Report
{
    public class DTODowntimeReason
    {
        public int Id { get; set; }
        public int ProductivityID { get; set; }
        public int DowntimeCodeId { get; set; }
        public int Flow { get; set; }
        public decimal Minutes { get; set; }
        public int DowntimeCategoryId { get; set; }
        public string SubCategory1 { get; set; } = default!;
        public string SubCategory2 { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Failure { get; set; } = default!;
    }
}