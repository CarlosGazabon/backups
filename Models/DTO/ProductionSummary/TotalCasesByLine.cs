using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class TotalCasesByLine
    {
        public string Line { get; set; } = default!;
        public int Production { get; set; }
        public decimal Percent { get; set; }
    }
}