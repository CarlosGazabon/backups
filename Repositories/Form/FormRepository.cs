using System.Globalization;
using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO.Form;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Form
{
  public class FormRepository : IFormRepository
  {
    private readonly ApplicationDBContext _context;

    public FormRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    /* -------------------------------- Functions ------------------------------- */
    public async Task<DTOSelectOptions> GetSelectOptions()
    {
      List<Line> lines = await _context.Line.Where(w => !w.Inactive).ToListAsync();
      List<Shift> shifts = await _context.Shift.Where(w => !w.Inactive).ToListAsync();
      List<Hour> hours = await _context.Hour.ToListAsync();
      List<Supervisor> supervisors = await _context.Supervisor.Where(w => !w.Inactive).ToListAsync();


      DTOSelectOptions result = new DTOSelectOptions
      {
        Lines = lines,
        Shifts = shifts,
        Hours = hours,
        Supervisors = supervisors
      };

      return result;
    }

    public async Task<DTOProductByLine> GetProductByLine(int idLine)
    {
      List<Product> products = await _context.Product.Where(w => w.LineID == idLine && !w.Inactive).ToListAsync();

      DTOProductByLine result = new DTOProductByLine
      {
        Products = products
      };

      return result;
    }

    public async Task<List<DowntimeCategory>> GetCategory()
    {
      List<DowntimeCategory> categories = await _context.DowntimeCategory.Where(w => !w.Inactive).ToListAsync();

      return categories;
    }

    public async Task<List<DowntimeSubCategory1>> GetSubcategory1(int idCategory)
    {
      List<DowntimeSubCategory1> subcategories = await _context.DowntimeSubCategory1.Where(w => w.DowntimeCategoryID == idCategory && !w.Inactive).ToListAsync();

      return subcategories;
    }

    public async Task<List<DowntimeSubCategory2>> GetSubcategory2(int idSubcategory1)
    {
      List<DowntimeSubCategory2> subcategories = await _context.DowntimeSubCategory2.Where(w => w.DowntimeSubCategory1ID == idSubcategory1 && !w.Inactive).ToListAsync();

      return subcategories;
    }

    public async Task<List<DowntimeCode>> GetCodes(int idSubcategory2)
    {
      List<DowntimeCode> codes = await _context.DowntimeCode.Where(w => w.DowntimeSubCategory2ID == idSubcategory2 && !w.Inactive).ToListAsync();

      return codes;
    }

    public async Task<Productivity> CreateRecord(Productivity record)
    {
      var productivityList = await _context.Productivity
      .Where(p => p.Date == Convert.ToDateTime(record.Date))
      .Where(p => p.LineID == record.LineID)
      .Where(p => p.ShiftID == record.ShiftID)
      .Where(p => p.HourID == record.HourID)
      .ToListAsync();
      if (productivityList.Count() > 0)
      {
        throw new Exception("400: Record already exists.");
      }

      // Start and end hour
      var hour = await _context.Hour.FindAsync(record.HourID);
      if (hour != null)
      {
        DateTimeFormatInfo dateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
        TimeSpan oneHour = new TimeSpan(1, 0, 0);
        DateTime dateTime;

        if (DateTime.TryParse(hour.Time, dateTimeFormatInfo, out dateTime))
        {
          dateTime = dateTime.Add(oneHour);

          // Print the resulting DateTime object.
          record.HourStart = $"{hour.Time}";
          record.HourEnd = $"{dateTime.ToString("hh:mm tt")}";
        }
      }

      // Add SKU and SKU2 in Productivity record
      for (int i = 0; i < record.ProductivityFlows.Count(); i++)
      {
        var flow = record.ProductivityFlows.ToList()[i];

        if (flow.ProductID == 0)
        {
          flow.ProductID = null;
        }
        if (flow.ProductID > 0)
        {
          var product = await _context.Product.FindAsync(flow.ProductID);
          if (product != null)
          {
            if (i == 0)
            {
              record.Sku = product.Sku;
            }
            if (i == 1)
            {
              record.Sku2 = product.Sku;
            }
          }
        }
      }

      // Add name supervisor
      var supervisor = await _context.Supervisor.FindAsync(record.SupervisorID);
      if (supervisor != null)
      {
        record.Name = supervisor.Name;
      }

      // Save
      _context.Productivity.Add(record);
      await _context.SaveChangesAsync();
      return record;
    }

    public async Task<Productivity> GetFormById(int id)
    {
      var productivity = await _context.Productivity
      .Include(e => e.ProductivityFlows).ThenInclude(pf => pf.Product)
      .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeCategory)
      .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeSubCategory1)
      .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeSubCategory2)
      .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeCode)
      .FirstOrDefaultAsync(i => i.Id == id) ?? throw new Exception("400: Record already exists.");
      return productivity;
    }

    //TODO:Needed rework GetWorkingProduct
    private async Task<Product?> GetWorkingProduct(ICollection<ProductivityFlow> flows)
    {
      int workingProductId = 0;
      var draftProduct = flows
      .Select(flow => _context.Product.FirstOrDefault(product => product.Id == flow.ProductID))
      .ToList();

      Product? workingProduct = null;

      if (draftProduct.Count == 1)
      {
        // Line with one flow

        workingProductId = draftProduct[0]?.Id ?? 0;
      }
      else if (draftProduct.Count == 2)
      {
        // Line with two flows

        int standardSpeedOne = draftProduct[0]?.StandardSpeed ?? 0;
        int standardSpeedTwo = draftProduct[1]?.StandardSpeed ?? 0;

        if (standardSpeedOne > standardSpeedTwo)
        {
          workingProductId = draftProduct[0]?.Id ?? 0;
        }
        else
        {
          workingProductId = draftProduct[1]?.Id ?? 0;
        }
      }

      workingProduct = await _context.Product.FindAsync(workingProductId);
      return workingProduct;
    }

    //TODO:Needed rework ProductivityExists
    private bool ProductivityExists(int id)
    {
      return (_context.Productivity?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    public async Task UpdateRecord(int id, Productivity productivity)
    {
      var productivityList = await _context.Productivity
      .Where(p => p.Id != id)
      .Where(p => p.Date == Convert.ToDateTime(productivity.Date))
      .Where(p => p.LineID == productivity.LineID)
      .Where(p => p.ShiftID == productivity.ShiftID)
      .Where(p => p.HourID == productivity.HourID)
      .ToListAsync();
      if (productivityList.Count() > 0)
      {
        throw new Exception("400: Record already exists.");
      }
      // Edit productivity
      if (id != productivity.Id)
      {
        throw new Exception("Id mismatch.");
      }

      int[] currentInts = _context.DowntimeReason.Where(x => x.ProductivityID == productivity.Id).Select(x => x.Id).ToArray();

      int[] payloadInts = productivity.DowntimeReasons.Where(x => x.ProductivityID == productivity.Id).Select(x => x.Id).ToArray();
      for (int i = 0; i < currentInts.Length; i++)
      {
        id = currentInts[i];
        if (!payloadInts.Contains(id))
        {
          // DELETE
          _context.DowntimeReason.Where(x => x.Id == id).ExecuteDelete();
        }
      }

      foreach (var reason in productivity.DowntimeReasons.ToArray())
      {
        if (reason.Id == 0)
        {
          _context.Entry(reason).State = EntityState.Added;
        }
        else
        {
          _context.Entry(reason).State = EntityState.Modified;
        }
      }

      await _context.SaveChangesAsync();

      var flowsToRemove = new List<ProductivityFlow>();
      foreach (var flow in productivity.ProductivityFlows.ToArray())
      {
        if (flow.ProductID == 0)
        {
          flowsToRemove.Add(flow);
        }
        else
        {
          _context.Entry(flow).State = EntityState.Modified;
        }
      }

      // Eliminar los registros después de la iteración
      foreach (var flow in flowsToRemove)
      {
        _context.ProductivityFlow.Remove(flow);
      }
      _context.Entry(productivity).State = EntityState.Modified;


      try
      {
        // Normalize Some Productivity and Flow Fields            
        for (int i = 0; i < productivity.ProductivityFlows.Count(); i++)
        {
          var flow = productivity.ProductivityFlows.ToList()[i];

          if (flow.ProductID == 0)
          {
            flow.ProductID = null;
          }
          if (flow.ProductID > 0)
          {
            var product = await _context.Product.FindAsync(flow.ProductID);
            if (product != null)
            {
              if (i == 0)
              {
                productivity.Sku = product.Sku;
              }
              if (i == 1)
              {
                productivity.Sku2 = product.Sku;
              }
            }

          }

        }

        var supervisor = await _context.Supervisor.FindAsync(productivity.SupervisorID);

        if (supervisor != null)
        {
          productivity.Name = supervisor.Name;
        }

        var hour = await _context.Hour.FindAsync(productivity.HourID);

        if (hour != null)
        {
          DateTimeFormatInfo dateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
          TimeSpan oneHour = new TimeSpan(1, 0, 0);
          DateTime dateTime;

          if (DateTime.TryParse(hour.Time, dateTimeFormatInfo, out dateTime))
          {
            dateTime = dateTime.Add(oneHour);

            // Print the resulting DateTime object.
            productivity.HourStart = $"{hour.Time}";
            productivity.HourEnd = $"{dateTime.ToString("hh:mm tt")}";
          }

        }


        // Define Working Product and Calculate Some Totals
        Product? workingProduct = await GetWorkingProduct(productivity.ProductivityFlows);

        var totalProduction = 0;
        totalProduction = productivity.ProductivityFlows.Aggregate(0, (acc, x) => acc + Decimal.ToInt32(x.Production));

        if (workingProduct != null)
        {
          var standardSpeed = workingProduct.StandardSpeed;
          var remainingProduction = standardSpeed - totalProduction;
          decimal remainingMinutes = (decimal)remainingProduction / standardSpeed * 60;

          productivity.RemainingProduction = remainingProduction;
          productivity.RemainingMinutes = remainingMinutes;
          productivity.StandardSpeed = standardSpeed;
        }

        // Calculate Total Downtime
        Decimal totalDowntimeMinutes = 0;
        totalDowntimeMinutes = productivity.DowntimeReasons.Aggregate(0, (Decimal acc, DowntimeReason x) => acc + x.Minutes);

        productivity.DowntimeMinutes = totalDowntimeMinutes;

        await _context.SaveChangesAsync();


      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ProductivityExists(id))
        {
          throw new Exception("400: Record already exists.");
        }
        else
        {
          throw;
        }
      }
    }
  }
}