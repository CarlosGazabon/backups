using ClosedXML.Excel;
using inventio.Repositories.Dashboards.CasesAnalysis;
using Inventio.Models.DTO.Cases;
using Microsoft.AspNetCore.Mvc;

namespace intenvio.Controllers
{
  [ApiController]
  [Route("api/cases-analysis")]
  public class CasesController : ControllerBase
  {
    private readonly ICasesAnalysisRepository _casesRepository;

    public CasesController(ICasesAnalysisRepository casesRepository)
    {
      _casesRepository = casesRepository;
    }

    //* --------------------------------- Filters -------------------------------- */
    [HttpPost("lines")]
    public async Task<IActionResult> GetLines(CasesFilter filter)
    {
      try
      {
        var lines = await _casesRepository.GetLines(filter);
        return Ok(lines);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    //* -------------------------------- Dashboard ------------------------------- */
    [HttpPost("dashboard")]
    public async Task<IActionResult> GetDashboard(CasesFilter filter)
    {
      try
      {
        var result = await _casesRepository.GetDashboard(filter);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }
    }

    //* ----------------------------- Generated Excel ---------------------------- */
    [HttpPost("excel-perline")]
    public async Task<ActionResult> ExcelCasesPerLine(CasesFilter filter)
    {
      try
      {
        var sheet = await _casesRepository.ExcelCasesPerLine(filter);

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