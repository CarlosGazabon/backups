using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class Standards
{
    public required int Id { get; set; }
    public required int Line { get; set; }
    public required string NetContent { get; set; }
    public required int StandardSpeed { get; set; }
    public required bool Inactive { get; set; }


}
