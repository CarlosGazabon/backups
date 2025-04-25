namespace inventio.Models.DTO.Productivity
{
    public class DTODowntimeReportExcel
    {
        public DateTime Date { get; set; }
        public string? HourStart { get; set; }
        public string? HourEnd { get; set; }
        public string? Shift { get; set; }
        public string? Line { get; set; }
        public string? Sku { get; set; }
        public string? Sku2 { get; set; }
        public string? Supervisor { get; set; }
        public string? Code { get; set; }
        public string? Category { get; set; }
        public string? Subcategory1 { get; set; }
        public string? Subcategory2 { get; set; }
        public decimal Minutes { get; set; }
        public int Flow { get; set; }
    }
}