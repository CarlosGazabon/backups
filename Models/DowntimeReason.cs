using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class DowntimeReason
{
    public int Id { get; set; }

    public int FlowIndex { get; set; }
    public int ProductivityID { get; set; }

    [ForeignKey("ProductivityID")]
    public virtual Productivity? Productivity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Minutes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExtraMinutes { get; set; } = 0;

    public int DowntimeCategoryId { get; set; }
    public virtual DowntimeCategory? DowntimeCategory { get; set; }

    public int DowntimeSubCategory1Id { get; set; }
    public virtual DowntimeSubCategory1? DowntimeSubCategory1 { get; set; }

    public int DowntimeSubCategory2Id { get; set; }
    public virtual DowntimeSubCategory2? DowntimeSubCategory2 { get; set; }

    public int DowntimeCodeId { get; set; }
    public virtual DowntimeCode? DowntimeCode { get; set; }
}
