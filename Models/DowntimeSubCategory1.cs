using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class DowntimeSubCategory1
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool Inactive { get; set; }
    public required int DowntimeCategoryID { get; set; }
    public virtual DowntimeCategory? DowntimeCategory { get; set; }

}
