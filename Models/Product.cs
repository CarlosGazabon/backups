using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = default!;

    // [Column(TypeName = "decimal(18,2)")]
    public string NetContent { get; set; } = default!;

    public string Packing { get; set; } = default!;
    public string Flavour { get; set; } = default!;

    public bool Inactive { get; set; }
    public int UnitsPerPackage { get; set; }
    public int StandardSpeed { get; set; }

    public int LineID { get; set; }
    public virtual Line? Line { get; set; }

}

