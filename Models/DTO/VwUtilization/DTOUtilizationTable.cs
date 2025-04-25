using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.VwUtilization
{
    public class DTOUtilizationTable
    {
        public string Line { get; set; } = default!;
        public decimal Utilization { get; set; }
        public decimal ChangeHrs { get; set; }
        public decimal NetHrs { get; set; }
        public decimal TotalHrs { get; set; }
    }
}