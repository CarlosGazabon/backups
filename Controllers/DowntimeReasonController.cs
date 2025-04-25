using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeReasonController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DowntimeReasonController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/DowntimeReason
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeReason>>> GetDowntimeReason()
        {
            if (_context.DowntimeReason == null)
            {
                return NotFound();
            }
            return await _context.DowntimeReason.ToListAsync();
        }

        // GET: api/DowntimeReason/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DowntimeReason>> GetDowntimeReason(int id)
        {
            if (_context.DowntimeReason == null)
            {
                return NotFound();
            }
            var downtimeReason = await _context.DowntimeReason.FindAsync(id);

            if (downtimeReason == null)
            {
                return NotFound();
            }

            return downtimeReason;
        }

        // PUT: api/DowntimeReason/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDowntimeReason(int id, DowntimeReason downtimeReason)
        {
            if (id != downtimeReason.Id)
            {
                return BadRequest();
            }

            _context.Entry(downtimeReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DowntimeReasonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DowntimeReason
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DowntimeReason>> PostDowntimeReason(DowntimeReason downtimeReason)
        {
            if (_context.DowntimeReason == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DowntimeReason'  is null.");
            }
            _context.DowntimeReason.Add(downtimeReason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDowntimeReason", new { id = downtimeReason.Id }, downtimeReason);
        }

        // DELETE: api/DowntimeReason/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDowntimeReason(int id)
        {
            if (_context.DowntimeReason == null)
            {
                return NotFound();
            }
            var downtimeReason = await _context.DowntimeReason.FindAsync(id);
            if (downtimeReason == null)
            {
                return NotFound();
            }

            _context.DowntimeReason.Remove(downtimeReason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DowntimeReasonExists(int id)
        {
            return (_context.DowntimeReason?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
