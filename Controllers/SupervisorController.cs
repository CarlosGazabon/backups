using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using inventio.Models.DTO;

namespace inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public SupervisorController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Supervisor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supervisor>>> GetSupervisor()
        {
            if (_context.Supervisor == null)
            {
                return NotFound();
            }
            return await _context.Supervisor.ToListAsync();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetSupervisorActive()
        {
            if (_context.Supervisor == null)
            {
                return NotFound();
            }
            return await _context.Supervisor
            .Where(w => w.Inactive == false)
                            .Select(s => new DTOReactDropdown<int>
                            {
                                Label = s.Description + " - " + s.Name,
                                Value = s.Id
                            })
            .ToListAsync();
        }

        // GET: api/Supervisor/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Supervisor>> GetSupervisor(int id)
        {
            if (_context.Supervisor == null)
            {
                return NotFound();
            }
            var supervisor = await _context.Supervisor.FindAsync(id);

            if (supervisor == null)
            {
                return NotFound();
            }

            return supervisor;
        }

        // PUT: api/Supervisor/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSupervisor(int id, Supervisor supervisor)
        {
            if (id != supervisor.Id)
            {
                return BadRequest();
            }

            _context.Entry(supervisor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupervisorExists(id))
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
            var supervisor = await _context.Supervisor.FindAsync(id);

            if (supervisor != null)
            {
                supervisor.Inactive = !supervisor.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/Supervisor
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Supervisor>> PostSupervisor(Supervisor supervisor)
        {
            if (_context.Supervisor == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Supervisor'  is null.");
            }
            _context.Supervisor.Add(supervisor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSupervisor", new { id = supervisor.Id }, supervisor);
        }

        // DELETE: api/Supervisor/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupervisor(int id)
        {
            var supervisor = await _context.Supervisor.FindAsync(id);
            if (supervisor == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    query = query.Where(q => q.SupervisorID == supervisor.Id);
                    List<Productivity> listDelete = query.ToList();

                    _context.Productivity.RemoveRange(listDelete);
                    _context.Supervisor.Remove(supervisor);
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

        private bool SupervisorExists(int id)
        {
            return (_context.Supervisor?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
