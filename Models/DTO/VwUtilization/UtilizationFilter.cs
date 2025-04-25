using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.VwUtilization
{
    public class UtilizationFilter
    {
        public string StartDate { get; set; } = default!;
        public string EndDate { get; set; } = default!;
        public List<string> Lines { get; set; } = default!;
        public List<string> Categories { get; set; } = default!;
        public string Time { get; set; } = default!;
    }
}