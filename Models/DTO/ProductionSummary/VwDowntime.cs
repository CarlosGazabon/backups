using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class Downtime
    {
        public string DownTimeCategory { get; set; } = default!;
        public decimal Hours { get; set; }
        public decimal Minutes { get; set; }
    }
}