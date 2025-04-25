using System;
using System.Collections.Generic;

namespace Inventio.Models.Views;

public class VwDowntime
{
    public DateTime Date { get; set; }

    public string Supervisor { get; set; } = null!;

    public string Line { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string Flavour { get; set; } = null!;

    public string NetContent { get; set; } = null!;

    public string Packing { get; set; } = null!;

    public string Shift { get; set; } = null!;

    public decimal Minutes { get; set; }
    public decimal ExtraMinutes { get; set; }

    public decimal? Hours { get; set; }

    public int FlowIndex { get; set; }

    public string Category { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Failure { get; set; } = null!;

    public string DowntimeSubCategory2 { get; set; } = null!;

    public int? ObjectiveMinutes { get; set; }

    public string HourStart { get; set; } = null!;

    public int Sort { get; set; }

    public string? HoraSort { get; set; }
}