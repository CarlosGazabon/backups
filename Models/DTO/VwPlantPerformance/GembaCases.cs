using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inventio.Models.DTO.ProductionSummary;

namespace inventio.Models.DTO.VwPlantPerformance
{
    public class GembaCases
    {
        public TotalCasesByLineFormatted TotalCasesByLine { get; set; } = default!;
    }
}