namespace inventio.Models.DTO.QualityIncident
{
    public class QualityIncidentReleaseDTO
    {
        public required int ReleasedForConsumption { get; set; }
        public required int ReleasedForDonation { get; set; }
        public required int ReleasedForDestruction { get; set; }
        public required int ReleasedForOther { get; set; }
        public required string ReleaseComments { get; set; }
        public required string ReleasedById { get; set; }
    }
}
