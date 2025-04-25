using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductivityFlowController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ProductivityFlowController(ApplicationDBContext context)
        {
            _context = context;
        }


        // GET: api/ProductivityFlow/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductivityFlow>> GetProductivityFlow(int id)
        {
            if (_context.ProductivityFlow == null)
            {
                return NotFound();
            }
            var productivityFlow = await _context.ProductivityFlow.FindAsync(id);

            if (productivityFlow == null)
            {
                return NotFound();
            }

            return productivityFlow;
        }

        // PUT: api/ProductivityFlow/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductivityFlow(int id, ProductivityFlow productivityFlow)
        {
            if (id != productivityFlow.Id)
            {
                return BadRequest();
            }

            _context.Entry(productivityFlow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductivityFlowExists(id))
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

        // POST: api/ProductivityFlow
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductivityFlow>> PostProductivityFlow(ProductivityFlow productivityFlow)
        {
            if (_context.ProductivityFlow == null)
            {
                return Problem("Entity set 'ApplicationDBContext.ProductivityFlow'  is null.");
            }
            _context.ProductivityFlow.Add(productivityFlow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductivityFlow", new { id = productivityFlow.Id }, productivityFlow);
        }

        // DELETE: api/ProductivityFlow/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductivityFlow(int id)
        {
            if (_context.ProductivityFlow == null)
            {
                return NotFound();
            }
            var productivityFlow = await _context.ProductivityFlow.FindAsync(id);
            if (productivityFlow == null)
            {
                return NotFound();
            }

            _context.ProductivityFlow.Remove(productivityFlow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductivityFlowExists(int id)
        {
            return (_context.ProductivityFlow?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
