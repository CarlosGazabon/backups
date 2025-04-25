namespace inventio.Repositories.Reports.ShiftProductionReport.Objects
{
    public class ProductionPerHour
    {
        public string? HourID { get; set; }
        public string? SKU { get; set; }
        public string? Standard { get; set; }
        public string? Flavor { get; set; }
        public string? Content { get; set; }
        public string? Production { get; set; }
        public string? ScrapUnits { get; set; }
        public string? ScrapPercentage { get; set; }
    }
}