using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;


public class Productivity
{

    public required int Id { get; set; }

    public required DateTime Date { get; set; }

    public required string? Name { get; set; }

    public string? Sku { get; set; }
    public int? ProductID { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Production { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal RemainingProduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal RemainingMinutes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal DowntimeMinutes { get; set; }

    public required string HourStart { get; set; }

    public required string HourEnd { get; set; }

    public required int Flow { get; set; }

    public DateTime? CreatedDate { get; set; }
    public required int ScrapUnits { get; set; }
    public required int CanScrap { get; set; } = 0;
    public required int PreformScrap { get; set; } = 0;
    public required int BottleScrap { get; set; } = 0;
    public required int PouchScrap { get; set; } = 0;

    // public required int Sku2 { get; set; }
    public string? Sku2 { get; set; }
    public int? ProductID2 { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Production2 { get; set; }

    public required int ScrapUnits2 { get; set; }

    public required int CanScrap2 { get; set; } = 0;
    public required int PreformScrap2 { get; set; } = 0;
    public required int BottleScrap2 { get; set; } = 0;
    public required int PouchScrap2 { get; set; } = 0;

    public required int StandardSpeed { get; set; }


    public required int SupervisorID { get; set; }

    public virtual Supervisor? Supervisor { get; set; }

    public required int LineID { get; set; }

    public Line? Line { get; set; }

    public required int ShiftID { get; set; }

    public virtual Shift? Shift { get; set; }

    public required int HourID { get; set; }

    public virtual Hour? Hour { get; set; }

    public string? Comment { get; set; }

    public virtual ICollection<ProductivityFlow> ProductivityFlows { get; set; } = default!;
    public virtual ICollection<DowntimeReason> DowntimeReasons { get; set; } = default!;

}

