namespace Inventio.Models
{
    public class ChangeOver
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Line { get; set; } = null!;

        public string Shift { get; set; } = null!;

        public string Supervisor { get; set; } = null!;

        public string Subcategory2 { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string Failure { get; set; } = null!;

        public int? Objective { get; set; }

        public decimal Minutes { get; set; }

        public int HourSort { get; set; }
    }
}