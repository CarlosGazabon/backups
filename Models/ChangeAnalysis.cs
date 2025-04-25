using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class ChangeAnalysis
{
    public required int Id { get; set; }
    public required DateTime Date { get; set; }
    public required string Line { get; set; }
    public required string Shift { get; set; }
    public required string Supervisor { get; set; }
    public required string Subcategory2 { get; set; }
    public required string Code { get; set; }
    public required string Failure { get; set; }
    public required int Objective { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Minutes { get; set; }
    public required int SortHour { get; set; }

}
