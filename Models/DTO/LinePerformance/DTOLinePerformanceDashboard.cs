using inventio.Models.DTO.Efficiency;
using inventio.Models.DTO.ProductivitySummary;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTOLinePerformanceDashboard
    {
        public DTOSummary? Summary { get; set; }
        public List<DTOProductionPerSku>? ProductionPerSku { get; set; }
        public List<DTOEfficiencyPerShift>? EfficiencyPerShift { get; set; }
        public DTODowntimePerMinutes? DowntimePerMinutes { get; set; }
        public List<DTOScrapPerLine>? ScrapPerLine { get; set; }

    }
}