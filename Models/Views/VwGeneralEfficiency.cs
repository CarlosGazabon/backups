namespace Inventio.Models.Views;

public partial class VwGeneralEfficiency
{
    public DateTime Date { get; set; }

    public int LineId { get; set; }

    public string Line { get; set; } = null!;

    public int ShiftId { get; set; }

    public string Shift { get; set; } = null!;

    public string? Sku { get; set; }

    public int ProductId { get; set; }

    public string Flavour { get; set; } = null!;

    public string Packing { get; set; } = null!;

    public string NetContent { get; set; } = null!;

    public int Flow { get; set; }

    public int SupervisorId { get; set; }

    public string Supervisor { get; set; } = null!;

    public decimal? Production { get; set; }

    public int? StandardSpeed { get; set; }

    public decimal ChangeHrs { get; set; }

    public decimal ChangeMins { get; set; }

    public int? Hrs { get; set; }

    public decimal? NetHrs { get; set; }

    public decimal? EffDen { get; set; }
}
