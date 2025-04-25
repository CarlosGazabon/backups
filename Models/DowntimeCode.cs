using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class DowntimeCode
{
    public int Id { get; set; }
    public required int DowntimeCategoryID { get; set; }
    public required int DowntimeSubCategory1ID { get; set; }
    public required int DowntimeSubCategory2ID { get; set; }
    public required string Code { get; set; }
    public required string Failure { get; set; }
    public required bool Inactive { get; set; }

    public virtual DowntimeCategory? DowntimeCategory { get; set; }
    public virtual DowntimeSubCategory1? DowntimeSubCategory1 { get; set; }
    public virtual DowntimeSubCategory2? DowntimeSubCategory2 { get; set; }

    public required int ObjectiveMinutes { get; set; } = 0;
}
