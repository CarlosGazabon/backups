using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventio.Models.DTO.ProductionSummary
{
    public class TotalCasesByShiftFormatted
    {
        public List<TotalCasesByShift> Data { get; set; } = default!;
        public int SumTotal { get; set; }
    }
}