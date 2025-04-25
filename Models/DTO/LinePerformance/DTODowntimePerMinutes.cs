using inventio.Models.DTO.DowntimeXSubcat;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTODowntimePerMinutes
    {
        public List<DTOMinutesXSubcategory>? MinutesXSubCategory { get; set; }
        public List<DTOMinutesXCode>? MinutesXCode { get; set; }
    }
}