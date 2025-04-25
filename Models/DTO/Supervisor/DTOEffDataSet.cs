namespace inventio.Models.DTO.Supervisor
{
    public class DTOEffDataSet
    {

        public string Supervisor { get; set; } = default!;
        public decimal? Production { get; set; }
        public decimal? EffDen { get; set; }
        public decimal? Hours { get; set; }
        public decimal? Flow { get; set; }
        public DateTime? Date { get; set; }
        public int? LineId { get; set; }
        public string? Shift { get; set; }
    }
}