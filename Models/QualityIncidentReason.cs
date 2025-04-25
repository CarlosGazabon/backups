using System.ComponentModel.DataAnnotations;

namespace Inventio.Models
{
    public class QualityIncidentReason
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public required string Description { get; set; }

        public bool Disable { get; set; }
    }
}