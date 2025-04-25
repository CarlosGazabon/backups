using ClosedXML.Excel;
using inventio.Models.DTO.Efficiency;
using inventio.Repositories.Dashboards.EfficiencyAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Inventio.Controllers
{
  [ApiController]
  [Route("api/efficiency-analysis")]
  public class EfficiencyController : ControllerBase
  {
    private readonly IEfficiencyRepository _efficiencyRepository;

    public EfficiencyController(IEfficiencyRepository efficiencyRepository)
    {
      _efficiencyRepository = efficiencyRepository;
    }

    //* --------------------------------- Filters -------------------------------- */
    [HttpPost("lines")]
    public async Task<IActionResult> GetLines(EfficiencyFilter filter)
    {
      try
      {
        var lines = await _efficiencyRepository.GetLines(filter);
        return Ok(lines);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    //* -------------------------------- Dashboard ------------------------------- */
    [HttpPost("dashboard")]
    public async Task<IActionResult> GetDashboard(EfficiencyFilter filter)
    {
      try
      {
        var result = await _efficiencyRepository.GetDashboard(filter);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    //* ----------------------------- Generated Excel ---------------------------- */
    [HttpPost("excel-perline")]
    public async Task<ActionResult> ExcelEfficiencyPerLine(EfficiencyFilter filter)
    {
      try
      {
        var sheet = await _efficiencyRepository.ExcelEfficiencyPerLine(filter);

        using XLWorkbook wb = new XLWorkbook();
        wb.Worksheets.Add(sheet);
        using MemoryStream stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "reportDowntimeCategory.xlsx");
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    [HttpPost("excel-perperiod")]
    public async Task<ActionResult> ExcelEfficiencyPerTimePeriod(EfficiencyFilter filter)
    {
      try
      {
        var sheet = await _efficiencyRepository.ExcelEfficiencyPerTimePeriod(filter);

        using XLWorkbook wb = new XLWorkbook();
        wb.Worksheets.Add(sheet);
        using MemoryStream stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "reportDowntimeCategory.xlsx");
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

  }
}