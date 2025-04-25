using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class TotalCasesByShift
    {
        public string Type { get; set; } = default!;
        public int Value { get; set; }
    }
}