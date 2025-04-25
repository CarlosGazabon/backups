using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.VwUtilization
{
    public class DTOUtilizationPerLine
    {
        public string Line { get; set; } = default!;
        public decimal Utilization { get; set; }
    }
}