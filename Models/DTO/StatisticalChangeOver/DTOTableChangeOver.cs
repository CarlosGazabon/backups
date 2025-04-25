namespace inventio.Models.DTO.StatisticalChangeOver
{
    public class DTOTableChangeOver
    {
        public string Code { get; set; } = default!;
        public string Failure { get; set; } = default!;
        public int? Objective { get; set; }
        public int Count { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Avg { get; set; }
        public decimal STDPopulation { get; set; }
        public decimal Total { get; set; }

    }
}