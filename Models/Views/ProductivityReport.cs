using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models.Views;

public class ProductivityReport
{
    public int? Id { get; set; }
    public DateTime Date { get; set; }
    public string? Line { get; set; }
    public string? Sku { get; set; }
    public string? Flavor { get; set; }
    public string? Shift { get; set; }
    public int? ShiftID { get; set; }
    public int? Production { get; set; }
    public int? StandardSpeed { get; set; }
    public string? HourStart { get; set; }
    public string? HourEnd { get; set; }
    public string? Supervisor { get; set; }
    public string? NetContent { get; set; }
    public string? Packing { get; set; }
    public int? Flow { get; set; }
    public int? LineID { get; set; }
    public int? SupervisorID { get; set; }
    public decimal? Downtime_minutes { get; set; }
    public string? Sku2 { get; set; }
    public string? Flavor2 { get; set; }
    public int? Production2 { get; set; }
    public string? NetContent2 { get; set; }
    public string? Packing2 { get; set; }
    public int? ProductID { get; set; }
    public int? ProductID2 { get; set; }
}