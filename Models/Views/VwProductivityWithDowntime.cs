namespace Inventio.Models.Views;

public partial class VwProductivityWithDowntime
{
    public DateTime Date { get; set; }

    public string Name { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public int? ProductId { get; set; }

    public int LineId { get; set; }

    public int? DowntimeCategoryId { get; set; }

    public string? DowntimeCategoryName { get; set; }

    public int? FlowIndex { get; set; }

    public decimal? Minutes { get; set; }
    public required string HourStart { get; set; }

    public required string HourEnd { get; set; }
}
