using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class DowntimeSubCategory2
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool Inactive { get; set; }

    public required int DowntimeSubCategory1ID { get; set; }
    public virtual DowntimeSubCategory1? DowntimeSubCategory1 { get; set; }

}
