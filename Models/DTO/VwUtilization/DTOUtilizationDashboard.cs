using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.VwUtilization
{
    public class DTOUtilizationDashboard
    {
        public DTOUtilization PlantUtilization { get; set; } = default!;
        public List<DTOUtilizationPerLine> UtilizationPerLine { get; set; } = default!;
        public List<DTOUtilizationTable> UtilizationTable { get; set; } = default!;
        public List<DTOUtilizationPerTime> UtilizationPerTime { get; set; } = default!;
    }
}