using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ShiftController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Shift
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShift()
        {
            if (_context.Shift == null)
            {
                return NotFound();
            }
            return await _context.Shift.ToListAsync();
        }

        [HttpGet("active-shift")]
        public async Task<ActionResult<IEnumerable<Shift>>> GetActiveShift()
        {
            if (_context.Shift == null)
            {
                return NotFound();
            }

            var result = await _context.Shift.Where(w => w.Inactive == false).ToListAsync();

            return result;
        }

        // GET: api/Shift/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Shift>> GetShift(int id)
        {
            if (_context.Shift == null)
            {
                return NotFound();
            }
            var shift = await _context.Shift.FindAsync(id);

            if (shift == null)
            {
                return NotFound();
            }

            return shift;
        }

        // PUT: api/Shift/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShift(int id, Shift shift)
        {
            if (id != shift.Id)
            {
                return BadRequest();
            }

            _context.Entry(shift).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(id))
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

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var shift = await _context.Shift.FindAsync(id);

            if (shift != null)
            {
                shift.Inactive = !shift.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/Shift
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Shift>> PostShift(Shift shift)
        {
            if (_context.Shift == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Shift'  is null.");
            }
            _context.Shift.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShift", new { id = shift.Id }, shift);
        }

        // DELETE: api/Shift/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shift.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    query = query.Where(q => q.ShiftID == shift.Id);
                    List<Productivity> listDelete = query.ToList();

                    _context.Productivity.RemoveRange(listDelete);
                    _context.Shift.Remove(shift);
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error transaction: " + ex);
                    return BadRequest("Error en la transacción: " + ex.Message);
                }
            }

            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return (_context.Shift?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
