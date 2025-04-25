namespace inventio.Models.DTO.QualityIncident
{
    public class QualityIncidentDTO
    {
        public required string IncidentNumber { get; set; }
        public string? IncidentNumberExtra { get; set; }
        public required string BatchNumber { get; set; }
        public required string ExpirationCode { get; set; }
        public int ProductId { get; set; }
        public int LineId { get; set; }
        public int ShiftId { get; set; }
        public int QualityIncidentReasonId { get; set; }
        public int Quantity { get; set; }
        public required string ActionComments { get; set; }
        public required string PalletsCodeNumber { get; set; }
        public required string Comments { get; set; }
        public required string PotencialRootCause { get; set; }
        public required string createdById { get; set; }
        public DateOnly DateOfIncident { get; set; }
    }
}