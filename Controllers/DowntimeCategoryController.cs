using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using inventio.Models.DTO;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeCategoryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DowntimeCategoryController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/DowntimeCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeCategory>>> GetDowntimeCategory()
        {
            if (_context.DowntimeCategory == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCategory.ToListAsync();
        }

        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetDowntimeCategories()
        {
            if (_context.DowntimeCategory == null)
            {
                return NotFound();
            }
            var categories = await _context.DowntimeCategory
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Name,
                    Value = s.Id
                })
                .ToListAsync();

            return categories;
        }

        [HttpGet("active-categories")]
        public async Task<ActionResult<IEnumerable<DowntimeCategory>>> GetActiveDowntimeCategory()
        {
            if (_context.DowntimeCategory == null)
            {
                return NotFound();
            }

            var result = await _context.DowntimeCategory.Where(w => w.Inactive == false).ToListAsync();

            return result;
        }

        // GET: api/DowntimeCategory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DowntimeCategory>> GetDowntimeCategory(int id)
        {
            if (_context.DowntimeCategory == null)
            {
                return NotFound();
            }
            var downtimeCategory = await _context.DowntimeCategory.FindAsync(id);

            if (downtimeCategory == null)
            {
                return NotFound();
            }

            return downtimeCategory;
        }

        // PUT: api/DowntimeCategory/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDowntimeCategory(int id, DowntimeCategory downtimeCategory)
        {
            if (id != downtimeCategory.Id)
            {
                return BadRequest();
            }

            _context.Entry(downtimeCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DowntimeCategoryExists(id))
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
            var category = await _context.DowntimeCategory.FindAsync(id);

            if (category != null)
            {
                category.Inactive = !category.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPut("{id}/toggleIsChangeOver")]
        public async Task<IActionResult> toggleIsChangeOver(int id)
        {
            var categories = await _context.DowntimeCategory.ToListAsync();
            categories.ForEach(s => s.IsChangeOver = false);
            _context.UpdateRange(categories);
            _context.SaveChanges();

            var category = await _context.DowntimeCategory.FindAsync(id);

            if (category != null)
            {
                category.IsChangeOver = !category.IsChangeOver;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/DowntimeCategory
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DowntimeCategory>> PostDowntimeCategory(DowntimeCategory downtimeCategory)
        {
            if (_context.DowntimeCategory == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DowntimeCategory'  is null.");
            }
            _context.DowntimeCategory.Add(downtimeCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDowntimeCategory", new { id = downtimeCategory.Id }, downtimeCategory);
        }

        // DELETE: api/DowntimeCategory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDowntimeCategory(int id)
        {
            var downtimeCategory = await _context.DowntimeCategory.FindAsync(id);
            if (downtimeCategory == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    IQueryable<DowntimeReason> reasons = _context.DowntimeReason;
                    IQueryable<DowntimeSubCategory1> categorie1 = _context.DowntimeSubCategory1;
                    IQueryable<DowntimeSubCategory2> categorie2 = _context.DowntimeSubCategory2;
                    IQueryable<DowntimeCode> codes = _context.DowntimeCode;

                    categorie1 = categorie1.Where(cat => cat.DowntimeCategoryID == downtimeCategory.Id);
                    List<DowntimeSubCategory1> listDeleteCat1 = categorie1.ToList();

                    List<int> listIdsDeleteCat1 = categorie1.Select(cat => cat.Id).ToList();
                    categorie2 = categorie2.Where(cat => listIdsDeleteCat1.Contains(cat.DowntimeSubCategory1ID));
                    List<DowntimeSubCategory2> listDeleteCat2 = categorie2.ToList();

                    codes = codes.Where(c => c.DowntimeCategoryID == downtimeCategory.Id);
                    List<DowntimeCode> listDeleteCodes = codes.ToList();

                    reasons = reasons.Where(q => q.DowntimeCategoryId == downtimeCategory.Id);
                    List<DowntimeReason> listReason = reasons.ToList();
                    List<int> productivityIds = listReason.Select(r => r.ProductivityID).ToList();
                    List<Productivity> listDelete = query.Where(p => productivityIds.Contains(p.Id)).ToList();

                    _context.DowntimeSubCategory1.RemoveRange(listDeleteCat1);
                    _context.DowntimeSubCategory2.RemoveRange(listDeleteCat2);
                    _context.DowntimeCode.RemoveRange(listDeleteCodes);
                    _context.Productivity.RemoveRange(listDelete);
                    _context.DowntimeCategory.Remove(downtimeCategory);
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

        private bool DowntimeCategoryExists(int id)
        {
            return (_context.DowntimeCategory?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
