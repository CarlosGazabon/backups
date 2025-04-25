using Microsoft.AspNetCore.Mvc;
using inventio.Repositories.Dashboards.DowntimeSku;
using inventio.Models.DTO.DowntimeSku;
using Inventio.Data;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/downtime-sku")]
    public class DowntimeSkuController : ControllerBase
    {
        private readonly IDowntimeSkuRepository _downtimeSkuRepository;

        public DowntimeSkuController(IDowntimeSkuRepository repository)
        {
            _downtimeSkuRepository = repository;
        }

        //* --------------------------------- Filter --------------------------------- */
        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<DTODowntimeSkuGetFilters>>> GetFilters(DTODowntimeSkuDatesFilter filters)
        {
            try
            {
                var filter = await _downtimeSkuRepository.GetFilters(filters);
                return Ok(filter);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }

        }

        //* -------------------------------- Dashboard ------------------------------- */
        [HttpPost("dashboard")]
        public async Task<ActionResult<DTODowntimeSkuDashboard>> GetDowntimeSkuDashboard([FromBody] DowntimeSkuFilter filters)
        {
            try
            {
                var result = await _downtimeSkuRepository.GetDowntimeSkuDashboard(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }
    }
}