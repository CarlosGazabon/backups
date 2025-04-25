using Inventio.Models.DTO.PackageSkuAnalysis;
using inventio.Repositories.Dashboards.PackageSku;
using Inventio.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace inventio
{
  [Route("api/packagesku")]
  [ApiController]
  public class PackageSkuController : ControllerBase
  {
    private readonly ApplicationDBContext _context;
    private readonly IPackageSkuRepository _packageSkuRepository;
    public PackageSkuController(ApplicationDBContext context, IPackageSkuRepository packageSkuRepository)
    {
      _context = context;
      _packageSkuRepository = packageSkuRepository;
    }


    [HttpPost("dropdown-line")]
    public async Task<IActionResult> GetLines(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetLine(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dropdown-sku")]
    public async Task<IActionResult> GetSKU(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetSKU(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dropdown-packing")]
    public async Task<IActionResult> GetPacking(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetPacking(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dropdown-netcontent")]
    public async Task<IActionResult> NetContent(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetNetContent(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }


    [HttpPost("dropdown-supervisors")]
    public async Task<IActionResult> GetSupervisors(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetSupervisors(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dashboard")]
    public async Task<IActionResult> GetPackageDashboard(PackageSkuFilter filters)
    {
      try
      {
        var result = await _packageSkuRepository.GetPackageDashboard(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("export-casespackage")]
    public async Task<IActionResult> ExportExcelCasesPerShift(PackageSkuFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.VwPackageSku
      .Where(w => w.Date >= startDate && w.Date <= endDate)
      .ToListAsync();

      var result = _packageSkuRepository.DataDonutPackage(data, filters);

      DataTable sheet = new DataTable("Production per Package");
      sheet.Columns.AddRange(new DataColumn[]
      {
        new DataColumn("Package"),
        new DataColumn("Production", typeof(int)),
      });

      foreach (var row in result.Data)
      {
        sheet.Rows.Add(
          row.Type,
          row.Production
        );
      }

      using (XLWorkbook wb = new XLWorkbook())
      {
        wb.Worksheets.Add(sheet);
        using (MemoryStream stream = new MemoryStream())
        {
          wb.SaveAs(stream);
          return File(stream.ToArray(),
              "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
              "reportProductivity.xlsx");
        }
      }
    }


  }

}