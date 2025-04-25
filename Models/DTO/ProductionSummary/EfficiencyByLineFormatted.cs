using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace inventio.Models.DTO.ProductionSummary
{
    public class EfficiencyByLineFormatted
    {
        public List<EfficiencyByLine> Table { get; set; } = default!;
        public decimal TotalEfficiency { get; set; }
    }
}