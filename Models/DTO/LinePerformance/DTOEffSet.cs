using inventio.Models.DTO.Efficiency;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTOEffSet
    {
        public List<DTOEfficiencyPerShift>? EfficiencyPerShift { get; set; }
        public decimal TotalEfficiency { get; set; }
    }
}