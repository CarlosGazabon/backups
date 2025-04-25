using inventio.Models.DTO;

namespace Inventio.Models
{
    public class DTOObjectiveRequest
    {
        public int Id { get; set; }
        public int Time { get; set; }
        public int LineID { get; set; }
        public int Production { get; set; }
        public decimal Utilization { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Scrap { get; set; }
    }
}