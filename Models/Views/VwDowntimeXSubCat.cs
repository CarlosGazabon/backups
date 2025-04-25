namespace Inventio.Models.Views;
public partial class VwDowntimeXSubCat
{
    public DateTime Date { get; set; }

    public int LineId { get; set; }

    public string Line { get; set; } = null!;

    public int ShiftId { get; set; }

    public string Shift { get; set; } = null!;

    public int CategoryId { get; set; }

    public string Category { get; set; } = null!;

    public int SubCategory2Id { get; set; }

    public string SubCategory2 { get; set; } = null!;

    public int FlowIndex { get; set; }

    public decimal Minutes { get; set; }
    public decimal ExtraMinutes { get; set; }

    public decimal? Hours { get; set; }

    public string Code { get; set; } = null!;

    public string Failure { get; set; } = null!;
}
