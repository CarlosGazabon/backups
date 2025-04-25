using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Inventio.Models;

namespace Inventio.Models
{
    public abstract class AuditableEntity
    {
        public required string AuditCreatedById { get; set; }
        public required string AuditUpdatedById { get; set; }

        public virtual User? AuditCreatedBy { get; set; }
        public virtual User? AuditUpdatedBy { get; set; }

        public required DateTime AuditDateCreated { get; set; }
        public required DateTime AuditDateUpdated { get; set; }
    }
    public class QualityIncident : AuditableEntity
    {
        public int Id { get; set; }
        public required DateOnly DateOfIncident { get; set; }
        public DateOnly? DateOfRelease { get; set; }

        [MaxLength(100)]
        public required string IncidentNumber { get; set; }

        [MaxLength(4)]
        public string? IncidentNumberExtra { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        [MaxLength(50)]
        public string? ExpirationCode { get; set; }

        public required int Quantity { get; set; }

        [MaxLength(270)]
        public required string ActionComments { get; set; }

        [MaxLength(270)]
        public required string PotencialRootCause { get; set; }

        [MaxLength(270)]
        public string? Comments { get; set; }
        public bool Released { get; set; }
        public virtual User? ReleasedBy { get; set; }
        public string? ReleasedById { get; set; }
        public int ReleasedForConsumption { get; set; }
        public int ReleasedForDonation { get; set; }
        public int ReleasedForDestruction { get; set; }
        public int ReleasedForOther { get; set; }

        [MaxLength(270)]
        public string? ReleaseComments { get; set; }

        public string? PalletsCodeNumber { get; set; }

        public int LineId { get; set; }

        public int ShiftId { get; set; }

        public int ProductId { get; set; }

        public int QualityIncidentReasonId { get; set; }

        public virtual Line? Line { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual Product? Product { get; set; }
        public virtual QualityIncidentReason? QualityIncidentReason { get; set; }
    }
}
