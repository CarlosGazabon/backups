using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class TotalCasesByDate
    {
        public string Time { get; set; } = default!;
        public string Type { get; set; } = default!;
        public int Value { get; set; }
    }
}