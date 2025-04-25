using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class ProductivityFlow
{
    public required int Id { get; set; }
    public required int FlowIndex { get; set; }

    public required int ProductivityID { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Minutes { get; set; }

    public int? ProductID { get; set; }
    public virtual Product? Product { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Production { get; set; }

    public required int ScrapUnits { get; set; }

    public required int CanScrap { get; set; } = 0;
    public required int PreformScrap { get; set; } = 0;
    public required int BottleScrap { get; set; } = 0;
    public int PouchScrap { get; set; } = 0;
    public required bool Empty { get; set; } = false;

    // public virtual DowntimeReason[]? DowntimeReasons { get; set; }

}

