// Date,Line,Shift,Sku,ProductId,Production,ScrapUnits
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models.Views;

public class ProductivitySummary
{
    public DateTime Date { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public required decimal Production { get; set; }
    public required int ScrapUnits { get; set; }
    public required int BottleScrap { get; set; }
    public required int CanScrap { get; set; }
    public required int PreformScrap { get; set; }
    public required int PouchScrap { get; set; }
    public required int StandardSpeed { get; set; }
    public required int UnitsPerPackage { get; set; }
    public required string Line { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public required decimal ScrapDEN { get; set; }
    public required string Shift { get; set; }
    public required string SKU { get; set; }
    public required int ProductId { get; set; }
    public required string Flavour { get; set; }
    public required string Packing { get; set; }
    public required string NetContent { get; set; }
    public required string Supervisor { get; set; }
    public required string SupervisorName { get; set; }
}