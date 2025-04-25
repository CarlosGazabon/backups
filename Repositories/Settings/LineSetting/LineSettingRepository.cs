using Inventio.Controllers;
using Inventio.Data;
using Inventio.Models;
using Inventio.Repositories.Settings.LineSetting;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories
{
    public class LineSettingRepository : ILineSettingRepository
    {
        private readonly ApplicationDBContext _context;

        public LineSettingRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Line>> GetLines()
        {
            if (_context.Line == null)
            {
                throw new Exception("Failed to get Hours");
            }
            return await _context.Line.ToListAsync();
        }

        public async Task<Line> GetLine(int id)
        {
            var line = await _context.Line.FindAsync(id) ?? throw new Exception($"Line with id {id} not found");
            return line;
        }

        public async Task<IEnumerable<LineController.LineSelect>> GetLineSelectFormatted()
        {
            var result = await _context.Line.ToListAsync();

            return result.Select(line => new LineController.LineSelect
            {
                Label = line.Name,
                Value = line.Name,
                Color = line.Color
            }).ToList();
        }

        public async Task<Line> UpdateLine(Line line)
        {
            _context.Entry(line).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var updatedLine = await _context.Line.FindAsync(line.Id) ?? throw new Exception($"Line with id {line.Id} not found");

            return updatedLine;
        }

        public async Task ToggleLineInactive(int id)
        {
            var line = await _context.Line.FindAsync(id);
            if (line != null)
            {
                line.Inactive = !line.Inactive;
                await _context.SaveChangesAsync();
            }
        }


        // public async Task ToggleLineCanScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);
        //     if (line != null)
        //     {
        //         line.CanScrap = !line.CanScrap;
        //         await _context.SaveChangesAsync();
        //     }
        // }

        // public async Task ToggleLinePreformScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);
        //     if (line != null)
        //     {
        //         line.PreformScrap = !line.PreformScrap;
        //         await _context.SaveChangesAsync();
        //     }
        // }

        // public async Task ToggleLineBottleScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);
        //     if (line != null)
        //     {
        //         line.BottleScrap = !line.BottleScrap;
        //         await _context.SaveChangesAsync();
        //     }
        // }

        // public async Task ToggleLinePouchScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);
        //     if (line != null)
        //     {
        //         line.PouchScrap = !line.PouchScrap;
        //         await _context.SaveChangesAsync();
        //     }
        // }

        public async Task<Line> AddLine(Line line)
        {
            _context.Line.Add(line);
            await _context.SaveChangesAsync();
            var newLine = await _context.Line.FindAsync(line.Id) ?? throw new Exception($"Line with id {line.Id} not found");

            return newLine;
        }

        public async Task<bool> DeleteLine(int id)
        {
            var line = await _context.Line.FindAsync(id) ?? throw new Exception($"Line with id {id} not found");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                IQueryable<Productivity> query = _context.Productivity;
                query = query.Where(q => q.LineID == line.Id);
                List<Productivity> listDelete = query.ToList();

                _context.Productivity.RemoveRange(listDelete);
                _context.Line.Remove(line);
                await _context.SaveChangesAsync();

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Error in transaction", ex);
            }
        }

        public bool LineExists(int id)
        {
            return _context.Line.Any(e => e.Id == id);
        }

    }
}
