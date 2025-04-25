using System.ComponentModel.DataAnnotations.Schema;

namespace inventio.Models.DTO.Report
{
    public class DTOProductivityList
    {
        public int Id { get; set; }
        public string Date { get; set; } = default!;
        public string Line { get; set; } = default!;
        public string Shift { get; set; } = default!;
        public string HourStart { get; set; } = default!;
        public string HourEnd { get; set; } = default!;
        public int HoursId { get; set; }
        public int HourSort { get; set; }
        public string Schedule { get; set; } = default!;
        public string SKU { get; set; } = default!;
        public int StandardSpeed { get; set; }
        public string Flavour { get; set; } = default!;
        public string NetContent { get; set; } = default!;
        public string Packing { get; set; } = default!;
        public decimal Production { get; set; }
        public decimal RemainingProduction { get; set; }
        public decimal RemainingMinutes { get; set; }
        public decimal DowntimeMinutes { get; set; }
        public int ScrapUnits { get; set; }
        public int Flow { get; set; }
        public string Comment { get; set; } = default!;
    }
}