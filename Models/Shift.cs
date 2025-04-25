using System.ComponentModel.DataAnnotations.Schema;

namespace Inventio.Models;

public class Shift
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string WeekDays1 { get; set; } = null!;

    public string WeekDays2 { get; set; } = null!;

    public string Schedule { get; set; } = null!;

    public string ScheduleStarts { get; set; } = null!;

    public string ScheduleEnds { get; set; } = null!;

    public bool Inactive { get; set; }

}
