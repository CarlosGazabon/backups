using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class ObjectiveChange
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public required string Failure { get; set; }
    public required string SubCategory2 { get; set; }
    public required int ObjectiveMin { get; set; }
    public required bool Inactive { get; set; }

}
