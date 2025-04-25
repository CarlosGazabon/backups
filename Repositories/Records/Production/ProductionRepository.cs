using inventio.Models.DTO;
using inventio.Models.DTO.Productivity;
using inventio.Repositories.Records.Production;
using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO.Productivity;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Records.Production
{
  public class ProductionRepository : IProductionRepository
  {
    private readonly ApplicationDBContext _context;

    public ProductionRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    //* --------------------------------- Filters -------------------------------- */
    public async Task<IEnumerable<DTOReactDropdown<int>>> GetLines(ProductivityFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);
      var result = await _context.Productivity
          .Where(w => w.Date >= startDate && w.Date <= endDate)
          .Select(s => new DTOReactDropdown<int>
          {
            Label = s.Line!.Name,
            Value = s.Line.Id
          })
          .Distinct()
          .OrderBy(o => o.Label)
          .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisor(ProductivityFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);
      var result = await _context.Productivity
          .Where(w => w.Date >= startDate && w.Date <= endDate && filters.Lines.Contains(w.LineID))
          .Select(s => new DTOReactDropdown<int>
          {
            Label = s.Supervisor!.Name,
            Value = s.Supervisor.Id
          })
          .Distinct()
          .OrderBy(o => o.Label)
          .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<DTOReactDropdown<int>>> GetShift(ProductivityFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);
      var result = await _context.Productivity
          .Where(w => w.Date >= startDate && w.Date <= endDate
            && filters.Lines.Contains(w.LineID)
            && filters.Supervisor.Contains(w.SupervisorID))
          .Select(s => new DTOReactDropdown<int>
          {
            Label = s.Shift!.Name,
            Value = s.Shift.Id
          })
          .Distinct()
          .OrderBy(o => o.Label)
          .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<DTOReactDropdown<int>>> GetSku(ProductivityFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);
      var data = await _context.Productivity
          .Where(w => w.Date >= startDate && w.Date <= endDate
            && filters.Lines.Contains(w.LineID)
            && filters.Supervisor.Contains(w.SupervisorID)
            && filters.Shift.Contains(w.ShiftID))
          .Select(s => new
          {
            s.Sku,
            s.Sku2,
            s.ProductID,
            s.ProductID2
          })
          .Distinct()
          .ToListAsync();

      List<DTOReactDropdown<int>> list = new List<DTOReactDropdown<int>>();

      foreach (var row in data)
      {
        string sku = row.Sku == null ? "0" : row.Sku.ToString();
        string sku2 = row.Sku2 == null ? "0" : row.Sku2.ToString();
        int skuID = row.ProductID.GetValueOrDefault(0);
        int skuID2 = row.ProductID2.GetValueOrDefault(0);


        list.Add(new DTOReactDropdown<int>()
        {
          Label = sku,
          Value = skuID
        });

        if (skuID != skuID2 && skuID2 != 0)
        {
          list.Add(new DTOReactDropdown<int>()
          {
            Label = sku2,
            Value = skuID2
          });
        }
      }

      List<DTOReactDropdown<int>> result = list.GroupBy(g => g.Value).Select(g => g.First()).ToList();

      return result;
    }

    //* ---------------------------------- Table --------------------------------- */
    public async Task<DTOResultRecords> GetProductivityRecords(ProductivityFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);


      IQueryable<Productivity> query = _context.Productivity
        .Include(i => i.Line)
        .Include(i => i.Shift)
        .Include(i => i.Supervisor)
        .Where(w => w.Date >= startDate && w.Date <= endDate);

      if (filters.Lines.Any())
      {
        query = query.Where(w => filters.Lines.Contains(w.LineID));
      }
      if (filters.Supervisor.Any())
      {
        query = query.Where(w => filters.Supervisor.Contains(w.SupervisorID));
      }
      if (filters.Shift.Any())
      {
        query = query.Where(w => filters.Shift.Contains(w.ShiftID));
      }
      if (filters.Sku.Any())
      {
        query = query.Where(w => filters.Sku.Contains((int)w.ProductID!) || filters.Sku.Contains((int)w.ProductID2!));
      }

      var data = await query
        .OrderBy(o => o.Date)
        .Skip(filters.Skip)
        .Take(filters.Take)
        .ToListAsync();

      int count = query.Count();

      DTOResultRecords result = new DTOResultRecords()
      {
        Count = count,
        List = data
      };

      return result;
    }

    //* ----------------------------- Generate Excel ----------------------------- */

  }
}