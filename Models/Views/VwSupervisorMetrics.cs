
namespace Inventio.Models.Views;

public partial class VwSupervisorMetrics
{
    public DateTime Date { get; set; }

    public int LineId { get; set; }

    public string Line { get; set; } = null!;

    public int ShiftId { get; set; }

    public string Shift { get; set; } = null!;

    public int SupervisorId { get; set; }

    public string Supervisor { get; set; } = null!;

    public decimal? Production { get; set; }

    public int? ScrapUnits { get; set; }

    public decimal? ScrapDen { get; set; }
}
