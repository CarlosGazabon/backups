using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTOLineObjectives
    {
        public decimal Utilization { get; set; }

        public decimal Efficiency { get; set; }

        public decimal Scrap { get; set; }
    }
}