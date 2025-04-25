using Microsoft.AspNetCore.Mvc;
using Inventio.Data;
using inventio.Repositories.Dashboards.GeneralDowntime;
using Inventio.Models.DTO.GeneralDowntime;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClosedXML.Excel;

namespace inventio
{
  [Route("api/generaldowntime")]
  [ApiController]
  public class GeneralDowntimeController : ControllerBase
  {
    private readonly IGeneralDowntimeRepository _generalDowntimeRepository;
    private readonly ApplicationDBContext _context;
    public GeneralDowntimeController(IGeneralDowntimeRepository generalDowntimeRepository, ApplicationDBContext context)
    {
      _generalDowntimeRepository = generalDowntimeRepository;
      _context = context;
    }

    [HttpPost("dropdown-line")]
    public async Task<IActionResult> GetLines(GeneralDowntimeFilter filters)
    {
      try
      {
        var result = await _generalDowntimeRepository.GetLine(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    //* Possible improvement
    /* [HttpPost("dropdown-categories")]
    public async Task<IActionResult> GetCategories(GeneralDowntimeFilter filters)
    {
      try
      {
        var result = await _generalDowntimeRepository.GetCategories(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    } */

    [HttpPost("dropdown-shifts")]
    public async Task<IActionResult> GetShifts(GeneralDowntimeFilter filters)
    {
      try
      {
        var result = await _generalDowntimeRepository.GetShifts(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dropdown-categories")]
    public async Task<IActionResult> GetCategories(GeneralDowntimeFilter filters)
    {
      try
      {
        var result = await _generalDowntimeRepository.GetCategories(filters);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("dashboard")]
    public async Task<IActionResult> GetDashboard(GeneralDowntimeFilter filters)
    {
      try
      {
        var result = await _generalDowntimeRepository.GetDashboard(filters, false);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("export-downtiemcategory")]
    public async Task<ActionResult> ExportExcelDowntimeperCategory(GeneralDowntimeFilter filters)
    {
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.VwDowntime
      .Where(w => w.Date >= startDate && w.Date <= endDate
        && filters.Lines!.Contains(w.Line)
        && filters.Shifts!.Contains(w.Shift))
      .Select(s => new { s.Line, s.Category, s.Minutes })
      .ToListAsync();

      List<DTOGeneralColumn> column = data
      .GroupBy(g => g.Category)
      .Select(g => new DTOGeneralColumn
      {
        Minutes = g.Sum(s => s.Minutes),
        Category = g.Key
      }).ToList();

      DataTable sheet = new DataTable("Downtime per Category");
      sheet.Columns.AddRange(new DataColumn[]
      {
        new DataColumn("Category"),
        new DataColumn("Downtime (min)"),
      });

      foreach (var row in column)
      {
        sheet.Rows.Add(
        row.Category,
        row.Minutes
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
              "reportDowntimeCategory.xlsx");
        }
      }

    }
  }
}