namespace inventio.Models.DTO.LinePerformance
{
    public class DTOEffDataSetPerLine
    {
        public string Line { get; set; } = default!;
        public decimal? Production { get; set; }
        public decimal? EffDen { get; set; }
        public decimal? Hours { get; set; }
    }
}