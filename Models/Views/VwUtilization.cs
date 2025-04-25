using System;
using System.Collections.Generic;

namespace Inventio.Models.Views;

public partial class VwUtilization
{
    public DateTime Date { get; set; }

    public string Name { get; set; } = null!;

    public int LineId { get; set; }

    public string Sku { get; set; } = null!;

    public int? ProductId { get; set; }

    public int Flow { get; set; }

    public int? Hrs { get; set; }

    public decimal ChangeHrs { get; set; }

    public decimal? NetHrs { get; set; }

    public decimal? NetPlantHrs { get; set; }
}
