using System.Data;
using inventio.Models.DTO.Cases;
using inventio.Utils;
using Inventio.Data;
using Inventio.Models.DTO.Cases;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.CasesAnalysis
{
  public class CasesAnalysisRepository : ICasesAnalysisRepository
  {
    private readonly ApplicationDBContext _context;

    public CasesAnalysisRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    //* ---------------------------- Methods Privates ---------------------------- */
    public decimal? TotalProduction(List<Models.Views.VwCases> data, CasesFilter filters)
    {
      decimal? totalProduction = data
        .Where(w => filters.Lines!.Contains(w.Line))
        .Sum(s => s.Production);

      return totalProduction;
    }

    public List<DTOCasesPerLine> CasesPerLine(List<Models.Views.VwCases> data, CasesFilter filters, decimal? total)
    {
      var rows = data
        .Where(w => filters.Lines!.Contains(w.Line))
        .GroupBy(g => g.Line)
        .Select(s => new
        {
          Line = s.Key,
          Production = s.Sum(s => s.Production)
        })
        .ToList();

      List<DTOCasesPerLine> result = new List<DTOCasesPerLine>();

      foreach (var row in rows)
      {
        result.Add(new DTOCasesPerLine
        {
          Line = row.Line,
          Production = (int)row.Production.GetValueOrDefault(0),
          Percent = Math.Round((decimal)(row.Production / total.GetValueOrDefault(1))! * 100, 2)
        });
      }
      ;

      return result;
    }

    public List<DTOCasesPerShift> CasesPerShift(List<Models.Views.VwCases> data, CasesFilter filters)
    {
      var result = data
        .Where(w => filters.Lines!.Contains(w.Line))
        .GroupBy(g => g.Shift)
        .Select(s => new DTOCasesPerShift
        {
          Shift = s.Key,
          Production = (int)s.Sum(s => s.Production).GetValueOrDefault(0)
        })
        .ToList();

      return result;
    }

    public List<DTOCasesPerTimePeriod> CasesPerTimePeriod(List<Models.Views.VwCases> data, CasesFilter filters)
    {
      Charts util = new();
      List<DTOCasesPerTimePeriod> result = new List<DTOCasesPerTimePeriod>();

      if (filters.Time == "Day")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.Line))
          .GroupBy(g => new
          {
            g.Date,
            g.Line
          })
          .Select(s => new
          {
            s.Key.Date,
            s.Key.Line,
            Production = s.Sum(su => su.Production).GetValueOrDefault(0)
          })
          .OrderBy(o => o.Date)
          .ToList();

        foreach (var row in draft)
        {
          result.Add(new()
          {
            Line = row.Line,
            Time = row.Date.ToString("MM/dd/yy"),
            Production = (int)row.Production
          });
        }
      }

      if (filters.Time == "Week")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.Line))
          .GroupBy(g => new
          {
            Year = g.Date.Year,
            Week = util.GetIso8601WeekOfYear(g.Date),
            g.Line
          })
          .Select(s => new
          {
            s.Key.Year,
            s.Key.Week,
            s.Key.Line,
            Production = s.Sum(su => su.Production).GetValueOrDefault(0)
          })
          .OrderBy(o => o.Year)
          .ThenBy(t => t.Week)
          .ToList();

        foreach (var row in draft)
        {
          result.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Week}/{row.Year}",
            Production = (int)row.Production
          });
        }
      }

      if (filters.Time == "Month")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.Line))
          .GroupBy(g => new
          {
            g.Date.Month,
            g.Date.Year,
            g.Line
          })
          .Select(s => new
          {
            s.Key.Year,
            s.Key.Month,
            s.Key.Line,
            Production = s.Sum(su => su.Production).GetValueOrDefault(0),
          })
          .OrderBy(o => o.Year)
          .ThenBy(t => t.Month)
          .ToList();

        foreach (var row in draft)
        {
          result.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Month}/{row.Year}",
            Production = (int)row.Production
          });
        }
      }

      if (filters.Time == "Quarter")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.Line))
          .GroupBy(g => new
          {
            Quarter = (g.Date.Month - 1) / 3 + 1,
            g.Date.Year,
            g.Line
          })
          .Select(s => new
          {
            s.Key.Year,
            s.Key.Quarter,
            s.Key.Line,
            Production = s.Sum(su => su.Production).GetValueOrDefault(0),
          })
          .OrderBy(o => o.Year)
            .ThenBy(t => t.Quarter)
            .ToList();

        foreach (var row in draft)
        {
          result.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Quarter}/{row.Year}",
            Production = (int)row.Production
          });
        }
      }

      if (filters.Time == "Year")
      {
        var draft = data
            .Where(w => filters.Lines!.Contains(w.Line))
            .GroupBy(g => new
            {
              g.Date.Year,
              g.Line
            })
            .Select(s => new
            {
              s.Key.Year,
              s.Key.Line,
              Production = s.Sum(su => su.Production).GetValueOrDefault(0),
            })
          .OrderBy(o => o.Year)
          .ToList();

        foreach (var row in draft)
        {
          result.Add(new()
          {
            Line = row.Line,
            Time = row.Year.ToString(),
            Production = (int)row.Production
          });
        }
      }


      return result;
    }

    //* --------------------------------- Filters -------------------------------- */
    public async Task<IEnumerable<string>> GetLines(CasesFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var lines = await _context.VwCases
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .Select(s => s.Line)
      .Distinct()
      .ToListAsync();

      return lines;
    }

    //* -------------------------------- Dashboard ------------------------------- */
    public async Task<DTOCasesDashboard> GetDashboard(CasesFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.VwCases
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .ToListAsync();

      decimal? totalProduction = TotalProduction(data, filters);
      List<DTOCasesPerLine> casesPerLine = CasesPerLine(data, filters, totalProduction);
      List<DTOCasesPerShift> casesPerShift = CasesPerShift(data, filters);
      List<DTOCasesPerTimePeriod> casesPerTimePeriod = CasesPerTimePeriod(data, filters);

      DTOCasesDashboard casesDashboard = new()
      {
        TotalProduction = (int)totalProduction.GetValueOrDefault(0),
        CasesPerLine = casesPerLine,
        CasesPerShift = casesPerShift,
        CasesPerTimePeriod = casesPerTimePeriod
      };

      return casesDashboard;
    }

    //* ----------------------------- Generated Excel ---------------------------- */
    public async Task<DataTable> ExcelCasesPerLine(CasesFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.VwCases
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .ToListAsync();

      decimal? totalProduction = TotalProduction(data, filters);
      List<DTOCasesPerLine> casesPerLine = CasesPerLine(data, filters, totalProduction);

      //* Sheet
      DataTable sheet = new DataTable("Cases per Line");
      sheet.Columns.AddRange(new DataColumn[] {
        new DataColumn("Line"),
        new DataColumn("Cases", typeof(int)),
        new DataColumn("Percent", typeof(double)),
      });

      //* Fill sheet
      foreach (var row in casesPerLine)
      {
        sheet.Rows.Add(
          row.Line,
          row.Production,
          row.Percent
        );
      }

      return sheet;
    }
  }
}