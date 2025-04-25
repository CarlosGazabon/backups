using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inventio.Models.DTO.ChangeOver;
using inventio.Models.DTO.ProductivitySummary;
using inventio.Models.DTO.VwUtilization;


namespace inventio.Models.DTO.VwPlantPerformance
{
    public class GembaObj
    {
        public GembaCases Cases { get; set; } = default!;
        public GembaEfficiency Efficiency { get; set; } = default!;
        public GembaDowntime Downtime { get; set; } = default!;
        public DTOUtilization Utilization { get; set; } = default!;
        public decimal TotalScrapPercentage { get; set; } = default!;
        public List<DTOScrapTable> ScrapTable { get; set; } = default!;
        public decimal ChangeOverMetrics { get; set; } = default!;
    }
}