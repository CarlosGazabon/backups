using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inventio.Models.DTO.ProductionSummary;

namespace inventio.Models.DTO.VwPlantPerformance
{
    public class GembaDowntime
    {
        public List<Downtime> Data { get; set; } = default!;
        public decimal TotalHours { get; set; }
        public decimal TotalMinutes { get; set; }
    }
}