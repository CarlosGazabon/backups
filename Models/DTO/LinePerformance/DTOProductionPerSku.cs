namespace inventio.Models.DTO.LinePerformance
{
    public class DTOProductionPerSku
    {
        public string Sku { get; set; } = default!;
        public decimal Production { get; set; }
    }
}