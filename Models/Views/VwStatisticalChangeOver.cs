namespace Inventio.Models.Views;

public partial class VwStatisticalChangeOver
{
    public DateTime Date { get; set; }

    public int SupervisorId { get; set; }

    public string Supervisor { get; set; } = null!;

    public string SupervisorName { get; set; } = null!;

    public int LineId { get; set; }

    public string Line { get; set; } = null!;

    public int ShiftId { get; set; }

    public string Shift { get; set; } = null!;

    public int SubCategory2Id { get; set; }

    public string SubCategory2 { get; set; } = null!;

    public int CodeId { get; set; }

    public string Code { get; set; } = null!;

    public decimal Minutes { get; set; }

    public decimal? Hours { get; set; }

    public int FlowIndex { get; set; }

    public string Failure { get; set; } = null!;

    public int? ObjectiveMinutes { get; set; }
}
