using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class EfficiencyByLine
    {
        public string Line { get; set; } = default!;
        public decimal Efficiency { get; set; }
    }
}