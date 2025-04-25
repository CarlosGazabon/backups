namespace inventio.Models.DTO.LinePerformance
{
    public class DTOProductionSet
    {
        public List<DTOProductionPerSku>? ProductionPerSku { get; set; }
        public decimal TotalProduction { get; set; }
    }
}