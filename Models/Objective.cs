using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models
{

    public class Objective
    {
        public int Id { get; set; }
        public required int LineID { get; set; }
        public required int Time { get; set; }
        public required int Production { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public required decimal Utilization { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public required decimal Efficiency { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public required decimal Scrap { get; set; }
        public virtual required Line Line { get; set; }
    }

}

