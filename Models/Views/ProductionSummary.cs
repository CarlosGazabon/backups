using System;
using System.Collections.Generic;

namespace Inventio.Models.Views;

public partial class ProductionSummary
{
    public DateTime Date { get; set; }

    public string Line { get; set; } = null!;

    public int LineId { get; set; }

    public string Shift { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string? ProductId { get; set; }

    public string Flavour { get; set; } = null!;

    public string Packing { get; set; } = null!;

    public string NetContent { get; set; } = null!;

    public string Supervisor { get; set; } = null!;
    public string SupervisorName { get; set; } = null!;

    public int Flow { get; set; }

    public decimal? Production { get; set; }

    public int? Scrap { get; set; }

    public int? StandardSpeed { get; set; }

    public decimal? Efficiency { get; set; }

    public int? Hrs { get; set; }

    public decimal ChangeHrs { get; set; }

    public decimal ChangeMins { get; set; }

    public decimal? NetHrs { get; set; }

    public decimal? EffDen { get; set; }

    public decimal? EffDensku { get; set; }
}
