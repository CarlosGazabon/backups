using System.ComponentModel.DataAnnotations;

namespace inventio.Utils
{
    public static class Enums
    {
        public enum DaysFilter
        {
            [Display(Name = "Day")]
            Day = 1,
            [Display(Name = "Week")]
            Week = 2,
            [Display(Name = "Month")]
            Month = 3,
            [Display(Name = "Quarter")]
            Quarter = 4,
            [Display(Name = "Year")]
            Year = 5
        }
    }
}