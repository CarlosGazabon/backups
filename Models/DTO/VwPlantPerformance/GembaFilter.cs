using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.VwPlantPerformance
{
    public class GembaFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<int> EfficiencyCategories { get; set; } = default!;
        public List<int> UtilizationCategories { get; set; } = default!;

    }
}