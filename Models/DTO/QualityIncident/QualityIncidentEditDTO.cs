namespace inventio.Models.DTO.QualityIncident
{
    public class QualityIncidentEditDTO
    {
        public string? IncidentNumberExtra { get; set; }
        public int LineId { get; set; }
        public int ShiftId { get; set; }

        public required string BatchNumber { get; set; }
        public required string ExpirationCode { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int QualityIncidentReasonId { get; set; }

        public required string ActionComments { get; set; }
        public required string PotencialRootCause { get; set; }
        public string? PalletsCodeNumber { get; set; }
        public required string Comments { get; set; }

        public required string EditById { get; set; }

        public DateOnly DateOfIncident { get; set; }
    }
}
