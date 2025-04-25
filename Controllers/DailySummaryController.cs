using Inventio.Models.DTO.DailySummary;
using inventio.Repositories.Dashboards.DailySummary;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
  [ApiController]
  [Route("api/dailysummary")]
  public class DailySummaryController : ControllerBase
  {
    private readonly IDailySummaryRepository _dailyRepository;
    public DailySummaryController(IDailySummaryRepository dailyRepository)
    {
      _dailyRepository = dailyRepository;
    }

    [HttpPost("dashboard")]
    public async Task<IActionResult> Dashboard(DailySummaryFilter filter)
    {
      try
      {
        var result = await _dailyRepository.GetDashboard(filter);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
      }

    }
  }


}