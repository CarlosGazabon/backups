using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Inventio.Models
{
    public class Standars
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Line { get; set; }

        public required string NetWeight { get; set; }

        [Required]
        public int StandarVelocity { get; set; }

        public bool Inactive { get; set; }
    }

}
