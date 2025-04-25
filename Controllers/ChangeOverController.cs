using inventio.Models.DTO.ChangeOver;
using inventio.Repositories.Dashboards.ChangeOver;
using Inventio.Data;
using Inventio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/changeover")]
    public class ChangeOverController : ControllerBase
    {

        private readonly ApplicationDBContext _context;
        private readonly IChangeOverRepository _changeOverRepository;

        public ChangeOverController(ApplicationDBContext context, IChangeOverRepository changeOverRepository)
        {
            _context = context;
            _changeOverRepository = changeOverRepository;
        }


        // GET: api/ChangeOver
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChangeOver>>> GetChangeOver()
        {
            if (_context.ChangeOver == null)
            {
                return NotFound();
            }
            return await _context.ChangeOver.Take(500).OrderByDescending(x => x.Date).ToListAsync();
        }

        [HttpPost("dropdown-line")]
        public async Task<IActionResult> GetChangeOverLines([FromBody] ChangeOverFilter filters)
        {
            if (_context.ChangeOver == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _changeOverRepository.GetChangeOverLines(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpPost("dropdown-shift")]
        public async Task<IActionResult> GetChangeOverShifts([FromBody] ChangeOverFilter filters)
        {
            if (_context.ChangeOver == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _changeOverRepository.GetChangeOverShifts(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpPost("dropdown-supervisor")]
        public async Task<IActionResult> GetChangeOverSupervisors([FromBody] ChangeOverFilter filters)
        {
            if (_context.ChangeOver == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _changeOverRepository.GetChangeOverSupervisors(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpPost("dropdown-subcategory")]
        public async Task<IActionResult> GetChangeOverSubCategory2([FromBody] ChangeOverFilter filters)
        {
            if (_context.ChangeOver == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _changeOverRepository.GetChangeOverSubCategory2(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        [HttpPost("dashboard")]
        public async Task<IActionResult> GetScrapDashboard([FromBody] ChangeOverFilter filters)
        {

            if (_context.ChangeOver == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _changeOverRepository.GetScrapDashboard(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }
    }
}