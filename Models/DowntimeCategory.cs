using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class DowntimeCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool Inactive { get; set; }

    public required bool IsChangeOver { get; set; }


}
