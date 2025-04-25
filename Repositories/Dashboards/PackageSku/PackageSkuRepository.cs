using inventio.Models.DTO;
using inventio.Models.DTO.Efficiency;
using inventio.Models.StoredProcedure;
using inventio.Models.Views;
using Inventio.Data;
using Inventio.Models.DTO.PackageSkuAnalysis;
using Inventio.Repositories.Formulas;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.PackageSku
{
  public class PackageSkuRepository : IPackageSkuRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IGeneralEfficiencyRepository _generalEfficiencyRepository;
    public PackageSkuRepository(ApplicationDBContext context, IGeneralEfficiencyRepository generalEfficiency)
    {
      _context = context;
      _generalEfficiencyRepository = generalEfficiency;
    }

    //* --------------------------------- Filters -------------------------------- */
    public async Task<IEnumerable<DTOReactDropdown<int>>> GetLine(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var result = await _context.VwPackageSku
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .Select(s => new DTOReactDropdown<int>
      {
        Label = s.Line,
        Value = s.LineID
      })
      .Distinct()
      .OrderBy(o => o.Label)
      .ToListAsync();

      return result;
    }


    public async Task<IEnumerable<DTOReactDropdown<string>>> GetPacking(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var result = await _context.VwPackageSku
      .Where(w => filters.Lines!.Contains(w.LineID) &&
      w.Date >= startDate && w.Date <= endDate)
      .Select(s => new DTOReactDropdown<string>
      {
        Label = s.Packing,
        Value = s.Packing
      })
      .Distinct()
      .OrderBy(o => o.Label)
      .ToListAsync();

      return result;
    }

    public async Task<IEnumerable<DTOReactDropdown<string>>> GetNetContent(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var result = await _context.VwPackageSku
      .Where(w => filters.Lines!.Contains(w.LineID) &&
      filters.Package!.Contains(w.Packing) &&
      w.Date >= startDate && w.Date <= endDate)
      .Select(s => new DTOReactDropdown<string>
      {
        Label = s.NetContent,
        Value = s.NetContent
      })
      .Distinct()
      .OrderBy(o => o.Label)
      .ToListAsync();

      return result;
    }


    public async Task<IEnumerable<DTOReactDropdown<string>>> GetSKU(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var result = await _context.VwPackageSku
      .Where(w => filters.Lines!.Contains(w.LineID) &&
      filters.Package!.Contains(w.Packing) &&
      filters.NetContent!.Contains(w.NetContent) &&
      w.Date >= startDate && w.Date <= endDate)
      .Select(s => new DTOReactDropdown<string>
      {
        Label = s.Sku,
        Value = s.Sku
      })
      .Distinct()
      .OrderBy(o => o.Label)
      .ToListAsync();

      return result;
    }


    public async Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisors(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var result = await _context.VwPackageSku
      .Where(w => filters.Lines!.Contains(w.LineID) &&
      filters.Package!.Contains(w.Packing) &&
      filters.NetContent!.Contains(w.NetContent) &&
      filters.Sku!.Contains(w.Sku) &&
      w.Date >= startDate && w.Date <= endDate)
      .Select(s => new DTOReactDropdown<int>
      {
        Label = s.Supervisor,
        Value = s.SupervisorID
      })
      .Distinct()
      .OrderBy(o => o.Label)
      .ToListAsync();

      return result;
    }


    /* Start charts */
    /* Package */
    public DTOPackageDonut DataDonutPackage(List<VwPackageSku> data, PackageSkuFilter filters)
    {
      var donut = data
      .Where(w => filters.Lines!.Contains(w.LineID)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorID))
      .GroupBy(g => new { g.Packing })
      .Select(s => new DTOPackageDonutData
      {
        Type = s.Key.Packing,
        Production = Convert.ToInt32(s.Sum(su => su.Production)),
        Percent = 0,
        Total = 0
      })
      .ToList();

      int sumTotal = donut.Sum(row => row.Production);

      foreach (var row in donut)
      {
        row.Percent = Math.Round((decimal)row.Production / Math.Max(sumTotal, 1) * 100, 2);
        row.Total = sumTotal;
      }

      DTOPackageDonut result = new()
      {
        Data = donut,
        SumTotal = sumTotal
      };
      return result;
    }

    public List<DTOPackageBar> DataBarPackage(List<VwPackageSku> data, PackageSkuFilter filters)
    {
      var bar = data
      .Where(w => filters.Lines!.Contains(w.LineID)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorID))
      .GroupBy(g => new { g.Packing, g.UnitsPerPackage })
      .Select(s => new DTOPackageBarData
      {
        Production = s.Sum(su => su.Production),
        BottleScrap = s.Sum(su => su.BottleScrap),
        CanScrap = s.Sum(su => su.CanScrap),
        PreformScrap = s.Sum(su => su.PreformScrap),
        UnitsPerPackage = s.Key.UnitsPerPackage,
        Package = s.Key.Packing
      })
      .ToList();

      List<DTOPackageBar> result = new List<DTOPackageBar>();

      foreach (var row in bar)
      {
        decimal GoodUnit = row.Production.GetValueOrDefault(0) * row.UnitsPerPackage;
        decimal TotalScrap = row.BottleScrap + row.CanScrap + row.PreformScrap;
        decimal TotalUnit = Math.Max(GoodUnit + TotalScrap, 1);
        decimal ScrapPercentage = Math.Round(TotalScrap / TotalUnit * 100, 2);
        //decimal? ScrapPercentage = Math.Round((decimal)(TotalScrap / Math.Max(TotalUnit.GetValueOrDefault(1) * 100, 1))!, 2);

        if (row.CanScrap > 0)
        {
          decimal scrapValue = row.CanScrap / TotalUnit * 100;
          result.Add(new DTOPackageBar
          {
            Type = "Can",
            Package = row.Package,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }
        if (row.BottleScrap > 0)
        {
          decimal scrapValue = row.PreformScrap / TotalUnit * 100;
          result.Add(new DTOPackageBar
          {
            Type = "Preform",
            Package = row.Package,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }
        if (row.PreformScrap > 0)
        {
          decimal scrapValue = row.BottleScrap / TotalUnit * 100;
          result.Add(new DTOPackageBar
          {
            Type = "Bottle",
            Package = row.Package,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }

      }

      return result;
    }

    public List<DTOPackageColumnData> DataColumnPackage(List<SPGetDataEfficiency> data, PackageSkuFilter filters)
    {
      var column = data
      .Where(w => filters.Lines!.Contains(w.LineId)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorId))
      .GroupBy(g => g.Packing)
      .Select(s => new
      {
        Package = s.Key,
        Production = s.Sum(su => su.Production),
        MaxProduction = s.Sum(su => su.MaxProduction)
      })
      .ToList();

      List<DTOPackageColumnData> result = new List<DTOPackageColumnData>();
      foreach (var row in column)
      {
        result.Add(new DTOPackageColumnData
        {
          Efficiency = Math.Round(row.Production / Math.Max(row.MaxProduction, 1) * 100, 2),
          Package = row.Package
        });
      }

      return result;
    }

    /* SKU */
    public DTOSkuDonut DataDonutSku(List<VwPackageSku> data, PackageSkuFilter filters)
    {
      var donut = data
      .Where(w => filters.Lines!.Contains(w.LineID)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorID))
      .GroupBy(g => new { g.Sku, g.Packing, g.Flavor, g.UnitsPerPackage, g.NetContent })
      .Select(s => new DTOSkuDonutData
      {
        Production = s.Sum(su => su.Production),
        Flavour = s.Key.Flavor,
        Sku = s.Key.Sku,
        Package = s.Key.Packing,
        NetContent = s.Key.NetContent,
        Percent = 0,
        Total = 0
      })
      .ToList();

      int sumTotal = (int)donut.Sum(row => row.Production ?? 0);
      foreach (var row in donut)
      {
        row.Percent = Math.Round((decimal)(row.Production ?? 0) / Math.Max(sumTotal, 1) * 100, 2);
        row.Total = sumTotal;
      }

      DTOSkuDonut result = new()
      {
        Data = donut,
        SumTotal = sumTotal
      };

      return result;
    }

    public List<DTOSkuBar> DataBarSku(List<VwPackageSku> data, PackageSkuFilter filters)
    {
      var bar = data
      .Where(w => filters.Lines!.Contains(w.LineID)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorID))
      .GroupBy(g => new
      {
        g.Sku,
        g.Packing,
        g.Flavor,
        g.UnitsPerPackage,
        g.NetContent
      })
      .Select(s => new DTOSkuBarData
      {
        Sku = s.Key.Sku,
        Production = s.Sum(su => su.Production),
        UnitsPerPackage = s.Key.UnitsPerPackage,
        BottleScrap = s.Sum(su => su.BottleScrap),
        CanScrap = s.Sum(su => su.CanScrap),
        PreformScrap = s.Sum(su => su.PreformScrap),
        Flavor = s.Key.Flavor,
        Package = s.Key.Packing,
        NetContent = s.Key.NetContent
      })
      .ToList();

      List<DTOSkuBar> result = new List<DTOSkuBar>();
      foreach (var row in bar)
      {
        decimal GoodUnit = row.Production.GetValueOrDefault(0) * row.UnitsPerPackage;
        decimal TotalScrap = row.BottleScrap + row.CanScrap + row.PreformScrap;
        decimal TotalUnit = Math.Max(GoodUnit + TotalScrap, 1);
        decimal ScrapPercentage = Math.Round(TotalScrap / TotalUnit * 100, 2);

        if (row.CanScrap > 0)
        {
          decimal scrapValue = row.CanScrap / TotalUnit * 100;
          result.Add(new DTOSkuBar
          {
            Type = "Can",
            Sku = row.Sku,
            Package = row.Package,
            Flavor = row.Flavor,
            NetContent = row.NetContent,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }
        if (row.PreformScrap > 0)
        {
          decimal scrapValue = row.PreformScrap / TotalUnit * 100;
          result.Add(new DTOSkuBar
          {
            Type = "Preform",
            Sku = row.Sku,
            Package = row.Package,
            Flavor = row.Flavor,
            NetContent = row.NetContent,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }
        if (row.BottleScrap > 0)
        {
          decimal scrapValue = row.BottleScrap / TotalUnit * 100;
          result.Add(new DTOSkuBar
          {
            Type = "Bottle",
            Sku = row.Sku,
            Package = row.Package,
            Flavor = row.Flavor,
            NetContent = row.NetContent,
            Scrap = Math.Round(scrapValue, 2),
            TotalScrap = TotalScrap,
            ScrapPercentage = ScrapPercentage
          });
        }
      }

      return result;
    }

    public List<DTOSkuColumnData> DataColumnSku(List<SPGetDataEfficiency> data, PackageSkuFilter filters)
    {
      var column = data
      .Where(w => filters.Lines!.Contains(w.LineId)
        && filters.Sku!.Contains(w.Sku)
        && filters.Package!.Contains(w.Packing)
        && filters.NetContent!.Contains(w.NetContent)
        && filters.Supervisors!.Contains(w.SupervisorId))
      .GroupBy(g => new
      {
        g.Sku,
        g.Packing,
        g.Flavor,
        g.NetContent
      })
      .Select(s => new
      {
        s.Key.Sku,
        Production = s.Sum(su => su.Production),
        MaxProduction = s.Sum(su => su.MaxProduction),
        s.Key.Flavor,
        Package = s.Key.Packing,
        s.Key.NetContent
      })
      .ToList();

      List<DTOSkuColumnData> result = new List<DTOSkuColumnData>();
      foreach (var row in column)
      {
        decimal? effDen = row.MaxProduction;
        if (effDen == 0 || effDen == null)
        {
          effDen = 1;
        }
        result.Add(new DTOSkuColumnData
        {
          Sku = row.Sku,
          Efficiency = Math.Round((row.Production / Math.Max(effDen.GetValueOrDefault(1), 1))! * 100, 2),
          Flavor = row.Flavor,
          Package = row.Package,
          NetContent = row.NetContent
        });
      }
      return result;
    }

    /* End Charts */

    public async Task<DTOPackageSkuDashboard> GetPackageDashboard(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.VwPackageSku
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .ToListAsync();

      var dataEff = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
      {
        StartDate = filters.StartDate,
        EndDate = filters.EndDate,
        Categories = filters.Categories
      });

      DTOPackageDonut packageDonut = DataDonutPackage(data, filters);
      List<DTOPackageBar> packageBar = DataBarPackage(data, filters);
      List<DTOPackageColumnData> packageColumn = DataColumnPackage(dataEff, filters);

      DTOSkuDonut skuDonut = DataDonutSku(data, filters);
      List<DTOSkuColumnData> skuColumn = DataColumnSku(dataEff, filters);
      List<DTOSkuBar> skuBar = DataBarSku(data, filters);

      DTOPackageSkuDashboard packageSkuDashboard = new()
      {
        PackageDonut = packageDonut,
        PackageBar = packageBar,
        PackageColumn = packageColumn,
        SkuDonut = skuDonut,
        SkuColumn = skuColumn,
        SkuBar = skuBar,
      };

      return packageSkuDashboard;
    }
  }
}