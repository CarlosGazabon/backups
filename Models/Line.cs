using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models
{
    public class Line
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Flow { get; set; }

        public bool Inactive { get; set; }

        public string Color { get; set; } = null!;

        public decimal Utilization { get; set; }

        public decimal Efficiency { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Scrap { get; set; }

        public bool? BottleScrap { get; set; }

        public bool? CanScrap { get; set; }

        public bool? PreformScrap { get; set; }

        public bool? PouchScrap { get; set; }

    }
}
