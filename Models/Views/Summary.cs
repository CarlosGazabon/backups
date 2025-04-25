// Date,Line,Shift,Sku,ProductId,Production,ScrapUnits
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models.Views;

public class Summary
{
    public DateTime Date { get; set; }
    public required string Line { get; set; }
    public required string Shift { get; set; }
    public required string Sku { get; set; }
    public required int ProductId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Production { get; set; }
    public required int ScrapUnits { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public required decimal Efficiency { get; set; }
    public required int StandardSpeed { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public required decimal EffDEN { get; set; }

}
