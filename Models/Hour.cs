using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class Hour
{
    public int Id { get; set; }
    public required string Time { get; set; }
    public required int TimeTypeID { get; set; }
    public required int Sort { get; set; }

}