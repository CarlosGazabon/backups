using System.Data;
using inventio.Models.StoredProcedure;
using inventio.Models.DTO;
using inventio.Models.DTO.Efficiency;
using inventio.Utils;
using Inventio.Data;
using Inventio.Repositories.Formulas;
using Microsoft.EntityFrameworkCore;
using ProjectNamespace.Utils;
using Inventio.Utils.Formulas;

namespace inventio.Repositories.Dashboards.EfficiencyAnalysis
{
  public class EfficiencyRepository : IEfficiencyRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IGeneralEfficiencyRepository _generalEfficiencyRepository;
    private readonly EfficiencyFormula _efficiencyFormula;


    public EfficiencyRepository(ApplicationDBContext context, IGeneralEfficiencyRepository formulaEfficiency, EfficiencyFormula efficiencyFormula)
    {
      _context = context;
      _generalEfficiencyRepository = formulaEfficiency;
      _efficiencyFormula = efficiencyFormula;
    }
    //* ---------------------------- Methods Privates ---------------------------- */
    public decimal TotalEfficiency(List<SPGetDataEfficiency> data, EfficiencyFilter filters)
    {
      var dataFilter = data
      .Where(w => filters.Lines!.Contains(w.LineId));

      decimal Production = dataFilter.Sum(s => s.Production);
      decimal MaxProduction = dataFilter.Sum(s => s.MaxProduction);

      decimal totalEfficiency = _efficiencyFormula.CalculateEfficiency(Production, MaxProduction);
      //decimal totalEfficiency = Production / Math.Max(MaxProduction, 1);

      return totalEfficiency;
    }

    public async Task<List<DTOEfficiencyPerLine>> EfficiencyPerLine(List<SPGetDataEfficiency> data, EfficiencyFilter filters)
    {
      var efficiencyPerLines = data
        .Where(w => filters.Lines!.Contains(w.LineId))
        .GroupBy(g => g.Line)
        .Select(g => new DTOEfficiencyPerLine
        {
          Line = g.Key,
          Efficiency = _efficiencyFormula.CalculateEfficiency(g.Sum(s => s.Production), g.Sum(s => s.MaxProduction)),
        })
        .ToList();

      var lines = await _context.Line.ToListAsync();

      efficiencyPerLines.ForEach(e =>
      {
        e.Goal = lines.FirstOrDefault(f => f.Name == e.Line)?.Efficiency ?? 0;
        e.Variance = e.Efficiency - e.Goal;
      });

      return efficiencyPerLines;
    }

    public List<DTOEfficiencyPerShift> EfficiencyPerShift(List<SPGetDataEfficiency> data, EfficiencyFilter filters)
    {
      var efficiencyPerShift = data
      .Where(w => filters.Lines!.Contains(w.LineId))
      .GroupBy(ps => ps.Shift)
      .Select(g => new DTOEfficiencyPerShift
      {
        Shift = g.Key,
        Efficiency = _efficiencyFormula.CalculateEfficiency(g.Sum(s => s.Production), g.Sum(s => s.MaxProduction)),
      })
      .OrderBy(o => o.Shift)
      .ToList();

      return efficiencyPerShift;
    }

    public List<DTOEfficiencyPerTimePeriod> EfficiencyPerTimePeriod(List<SPGetDataEfficiency> data, EfficiencyFilter filters)
    {
      Charts util = new();
      List<DTOEfficiencyPerTimePeriod> efficiencyPerTimePeriod = new List<DTOEfficiencyPerTimePeriod>();

      if (filters.Time == "Day")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.LineId))
          .GroupBy(g => new
          {
            g.Date,
            g.Line
          })
          .Select(s => new
          {
            s.Key.Date,
            s.Key.Line,
            Production = s.Sum(su => su.Production),
            MaxProduction = s.Sum(su => su.MaxProduction)
          })
          .OrderBy(o => o.Date)
          .ToList();

        foreach (var row in draft)
        {
          efficiencyPerTimePeriod.Add(new()
          {
            Line = row.Line,
            Time = row.Date.ToString("MM/dd/yy"),
            Efficiency = _efficiencyFormula.CalculateEfficiency(row.Production, row.MaxProduction, true, true)
          });
        }
      }

      if (filters.Time == "Week")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.LineId))
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
            Production = s.Sum(su => su.Production),
            MaxProduction = s.Sum(su => su.MaxProduction)
          })
          .OrderBy(o => o.Year)
          .ThenBy(t => t.Week)
          .ToList();

        foreach (var row in draft)
        {
          efficiencyPerTimePeriod.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Week}/{row.Year}",
            Efficiency = _efficiencyFormula.CalculateEfficiency(row.Production, row.MaxProduction, true, true)
          });
        }
      }

      if (filters.Time == "Month")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.LineId))
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
            Production = s.Sum(su => su.Production),
            MaxProduction = s.Sum(su => su.MaxProduction)
          })
          .OrderBy(o => o.Year)
          .ThenBy(t => t.Month)
          .ToList();
        foreach (var row in draft)
        {
          efficiencyPerTimePeriod.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Month}/{row.Year}",
            Efficiency = _efficiencyFormula.CalculateEfficiency(row.Production, row.MaxProduction, true, true)
          });
        }
      }

      if (filters.Time == "Quarter")
      {
        var draft = data
            .Where(w => filters.Lines!.Contains(w.LineId))
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
              Production = s.Sum(su => su.Production),
              MaxProduction = s.Sum(su => su.MaxProduction)
            })
            .OrderBy(o => o.Year)
            .ThenBy(t => t.Quarter)
            .ToList();

        foreach (var row in draft)
        {
          efficiencyPerTimePeriod.Add(new()
          {
            Line = row.Line,
            Time = $"{row.Quarter}/{row.Year}",
            Efficiency = _efficiencyFormula.CalculateEfficiency(row.Production, row.MaxProduction, true, true)
          });
        }
      }

      if (filters.Time == "Year")
      {
        var draft = data
          .Where(w => filters.Lines!.Contains(w.LineId))
          .GroupBy(g => new
          {
            g.Date.Year,
            g.Line
          })
          .Select(s => new
          {
            s.Key.Year,
            s.Key.Line,
            Production = s.Sum(su => su.Production),
            MaxProduction = s.Sum(su => su.MaxProduction)
          })
          .OrderBy(o => o.Year)
          .ToList();

        foreach (var row in draft)
        {
          efficiencyPerTimePeriod.Add(new()
          {
            Line = row.Line,
            Time = row.Year.ToString(),
            Efficiency = _efficiencyFormula.CalculateEfficiency(row.Production, row.MaxProduction, true, true)
          });
        }
      }

      return efficiencyPerTimePeriod;
    }


    //* --------------------------------- Filters -------------------------------- */
    public async Task<IEnumerable<DTOReactDropdown<int>>> GetLines(EfficiencyFilter filters)
    {
      var data = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      });

      var lines = data.Select(s => new DTOReactDropdown<int>
      {
        Label = s.Line,
        Value = s.LineId
      }).Distinct(new DTOReactDropdownComparer()).ToList();

      return lines;
    }

    //* -------------------------------- Dashboard ------------------------------- */
    public async Task<DTOEfficiencyDashboard> GetDashboard(EfficiencyFilter filters)
    {
      var data = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      });

      decimal totalEfficiency = TotalEfficiency(data, filters);
      List<DTOEfficiencyPerLine> efficiencyPerLine = await EfficiencyPerLine(data, filters);
      List<DTOEfficiencyPerShift> efficiencyPerShift = EfficiencyPerShift(data, filters);
      List<DTOEfficiencyPerTimePeriod> efficiencyPerTimePeriod = EfficiencyPerTimePeriod(data, filters);

      DTOEfficiencyDashboard efficiencyDashboard = new()
      {
        TotalEfficiency = totalEfficiency,
        EfficiencyPerLine = efficiencyPerLine,
        EfficiencyPerShift = efficiencyPerShift,
        EfficiencyPerTimePeriod = efficiencyPerTimePeriod
      };

      return efficiencyDashboard;
    }

    //* ---------------------------- Generated Exdcel ---------------------------- */
    public async Task<DataTable> ExcelEfficiencyPerLine(EfficiencyFilter filters)
    {
      var data = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      });

      List<DTOEfficiencyPerLine> efficiencyPerLine = await EfficiencyPerLine(data, filters);

      //* Sheet
      DataTable sheet = new DataTable("Efficiency per Line");
      sheet.Columns.AddRange(new DataColumn[] {
        new DataColumn("Line"),
        new DataColumn("Efficiency")
      });

      //* Fill sheet
      foreach (var row in efficiencyPerLine)
      {
        sheet.Rows.Add(
          row.Line,
          Math.Round(row.Efficiency * 100, 2)
        );
      }

      return sheet;
    }

    public async Task<DataTable> ExcelEfficiencyPerTimePeriod(EfficiencyFilter filters)
    {
      var data = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      });

      List<DTOEfficiencyPerTimePeriod> efficiencyPerTimePeriod = EfficiencyPerTimePeriod(data, filters);

      //* Sheet
      DataTable sheet = new DataTable($"Efficiency per {filters.Time}");
      sheet.Columns.AddRange(new DataColumn[] {
        new DataColumn($"Date per {filters.Time}"),
        new DataColumn("Line"),
        new DataColumn("Efficiency", typeof(double))
      });

      //* Fill sheet
      foreach (var row in efficiencyPerTimePeriod)
      {
        sheet.Rows.Add(
          row.Time,
          row.Line,
          row.Efficiency
        );
      }

      return sheet;
    }

    public async Task<List<SPGetDataEfficiency>> Test(EfficiencyFilter filters)
    {
      DTOGetEfficiencyFilter filterEfficiency = new()
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      };
      var data = await _generalEfficiencyRepository.GetEfficiency(filterEfficiency);

      return data;
    }


  }
}