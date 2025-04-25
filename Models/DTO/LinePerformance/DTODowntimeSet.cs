using inventio.Models.DTO.DowntimeXSubcat;

namespace inventio.Models.DTO.LinePerformance
{
    public class DTODowntimeSet
    {
        public List<DTOMinutesXSubcategory>? MinutesXSubcategories { get; set; }
        public decimal TotalDowntime { get; set; }
    }
}