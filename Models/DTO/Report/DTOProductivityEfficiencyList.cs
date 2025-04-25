namespace inventio.Models.DTO.Report
{
    public class DTOProductivityEfficiencyList
    {
        public DateTime Date { get; set; }
        public string Line { get; set; } = default!;
        public string Shift { get; set; } = default!;
        public string SKU { get; set; } = default!;
        public int ProductId { get; set; }
        public decimal Production { get; set; }
        public int StandardSpeed { get; set; }
        public decimal Efficiency { get; set; }
        public int Hrs { get; set; }
        public decimal ChangeHrs { get; set; }
        public decimal ChangeMins { get; set; }
        public decimal NetHrs { get; set; }
        public decimal EffDEN { get; set; }
        public decimal EffDENSKU { get; set; }
        public int Scrap { get; set; }
    }
}