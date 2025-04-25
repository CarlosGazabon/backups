
using Inventio.Controllers;
using Inventio.Models;

namespace Inventio.Repositories.Settings.LineSetting
{
    public interface ILineSettingRepository
    {
        Task<IEnumerable<Line>> GetLines();
        Task<Line> GetLine(int id);
        Task<IEnumerable<LineController.LineSelect>> GetLineSelectFormatted();
        Task<Line> UpdateLine(Line line);
        Task ToggleLineInactive(int id);
        // Task ToggleLineCanScrap(int id);
        // Task ToggleLinePreformScrap(int id);
        // Task ToggleLineBottleScrap(int id);
        // Task ToggleLinePouchScrap(int id);
        Task<Line> AddLine(Line line);
        Task<bool> DeleteLine(int id);
        bool LineExists(int id);
    }
}
