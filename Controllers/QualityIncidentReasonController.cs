using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QualityIncidentReasonController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public QualityIncidentReasonController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet("paginated")]
        public async Task<ActionResult> GetIncidentReasonPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }

            var totalRecords = await _context.QualityIncidentReason.CountAsync();
            var paginatedResult = await _context.QualityIncidentReason
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return Ok(new { TotalRecords = totalRecords, Records = paginatedResult });
        }

        // GET: api/QualityIncidentReason
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QualityIncidentReason>>> GetQualityIncidentReason()
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }
            return await _context.QualityIncidentReason.ToListAsync();
        }

        // GET: api/QualityIncidentReason/active
        [HttpGet("active-reasons")]
        public async Task<ActionResult<IEnumerable<QualityIncidentReason>>> GetActiveQualityIncidentReasons()
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }

            var activeIncidentReasons = await _context.QualityIncidentReason
                .Where(qir => !qir.Disable)
                .ToListAsync();

            return Ok(activeIncidentReasons);
        }

        // GET: api/QualityIncidentReason/active-lines
        [HttpGet("active-lines")]
        public async Task<ActionResult<IEnumerable<Line>>> GetActiveLines()
        {
            if (_context.Line == null)
            {
                return NotFound();
            }

            var activeLines = await _context.Line
                .Where(l => !l.Inactive)
                .ToListAsync();

            return Ok(activeLines);
        }

        // GET: api/QualityIncidentReason/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QualityIncidentReason>> GetQualityIncidentReason(int id)
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }
            var qualityIncidentReason = await _context.QualityIncidentReason.FindAsync(id);

            if (qualityIncidentReason == null)
            {
                return NotFound();
            }

            return qualityIncidentReason;
        }

        // PUT: api/QualityIncidentReason/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQualityIncidentReason(int id, QualityIncidentReason qualityIncidentReason)
        {
            if (id != qualityIncidentReason.Id)
            {
                return BadRequest();
            }

            _context.Entry(qualityIncidentReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QualityIncidentReasonExists(id))
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

        // POST: api/QualityIncidentReason
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<QualityIncidentReason>> PostQualityIncidentReason(QualityIncidentReason qualityIncidentReason)
        {
            if (_context.QualityIncidentReason == null)
            {
                return Problem("Entity set 'ApplicationDBContext.QualityIncidentReason'  is null.");
            }
            _context.QualityIncidentReason.Add(qualityIncidentReason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQualityIncidentReason", new { id = qualityIncidentReason.Id }, qualityIncidentReason);
        }

        // DELETE: api/QualityIncidentReason/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQualityIncidentReason(int id)
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }
            var qualityIncidentReason = await _context.QualityIncidentReason.FindAsync(id);
            if (qualityIncidentReason == null)
            {
                return NotFound();
            }

            _context.QualityIncidentReason.Remove(qualityIncidentReason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QualityIncidentReasonExists(int id)
        {
            return (_context.QualityIncidentReason?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // PATCH: api/QualityIncidentReason/5/toggle
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleQualityIncidentReason(int id)
        {
            if (_context.QualityIncidentReason == null)
            {
                return NotFound();
            }

            var qualityIncidentReason = await _context.QualityIncidentReason.FindAsync(id);
            if (qualityIncidentReason == null)
            {
                return NotFound();
            }

            qualityIncidentReason.Disable = !qualityIncidentReason.Disable;

            _context.Entry(qualityIncidentReason).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
