using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using inventio.Models.DTO.Settings.SubCategory2;
using inventio.Models.DTO;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeSubCategory2Controller : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DowntimeSubCategory2Controller(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/DowntimeSubCategory2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeSubCategory2>>> GetDowntimeSubCategory2()
        {
            if (_context.DowntimeSubCategory2 == null)
            {
                return NotFound();
            }
            return await _context.DowntimeSubCategory2.Include(e => e.DowntimeSubCategory1).ToListAsync();
        }

        [HttpGet("table")]
        public async Task<ActionResult<IEnumerable<SubCategory2WithSubCategory1DTO>>> GetDowntimeSubCategory2Table()
        {
            if (_context.DowntimeSubCategory1 == null)
            {
                return NotFound();
            }

            var subCategories = await _context.DowntimeSubCategory2
                .Include(e => e.DowntimeSubCategory1)
                .Select(sc => new SubCategory2WithSubCategory1DTO
                {
                    SubCategory2 = sc.Name,
                    SubCategory2Id = sc.Id,
                    SubCategory1 = sc.DowntimeSubCategory1.Name,
                    SubCategory1Id = sc.DowntimeSubCategory1ID,
                    Category = sc.DowntimeSubCategory1.DowntimeCategory.Name,
                    CategoryId = sc.DowntimeSubCategory1.DowntimeCategoryID,
                    Inactive = sc.Inactive
                })
                .ToListAsync();

            return Ok(subCategories);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IEnumerable<SubCategory2WithSubCategory1DTO>> GetSubCategories2WithCategoryAsync(int categoryId)
        {
            return await _context.DowntimeSubCategory2
                .Include(sc2 => sc2.DowntimeSubCategory1)
                .Where(sc2 => sc2.DowntimeSubCategory1.DowntimeCategoryID == categoryId)
                .Select(sc2 => new SubCategory2WithSubCategory1DTO
                {
                    SubCategory2Id = sc2.Id,
                    SubCategory2 = sc2.Name,
                    SubCategory1 = sc2.DowntimeSubCategory1.Name,
                    SubCategory1Id = sc2.DowntimeSubCategory1ID,
                    Category = sc2.DowntimeSubCategory1.DowntimeCategory.Name,
                    CategoryId = sc2.DowntimeSubCategory1.DowntimeCategoryID,
                    Inactive = sc2.Inactive
                })
                .ToListAsync();
        }

        [HttpGet("category/{categoryId}/subcategory1/{subCategory1Id}")]
        public async Task<IEnumerable<SubCategory2WithSubCategory1DTO>> GetSubCategories2WithCategoryAndSubCategoryAsync(int categoryId, int subCategory1Id)
        {
            return await _context.DowntimeSubCategory2
                .Include(sc2 => sc2.DowntimeSubCategory1)
                .Where(sc2 => sc2.DowntimeSubCategory1.DowntimeCategoryID == categoryId && sc2.DowntimeSubCategory1ID == subCategory1Id)
                .Select(sc2 => new SubCategory2WithSubCategory1DTO
                {
                    SubCategory2Id = sc2.Id,
                    SubCategory2 = sc2.Name,
                    SubCategory1 = sc2.DowntimeSubCategory1.Name,
                    SubCategory1Id = sc2.DowntimeSubCategory1ID,
                    Category = sc2.DowntimeSubCategory1.DowntimeCategory.Name,
                    CategoryId = sc2.DowntimeSubCategory1.DowntimeCategoryID,
                    Inactive = sc2.Inactive
                })
                .ToListAsync();
        }

        // GET: api/DowntimeSubCategory2/category/{categoryId}/subcategory1/{subCategory1Id}/subcategory2/{subCategory2Id}
        [HttpGet("category/{categoryId}/subcategory1/{subCategory1Id}/subcategory2/{subCategory2Id}")]
        public async Task<IEnumerable<SubCategory2WithSubCategory1DTO>> GetSubCategories2WithCategoryAndSubCategory1AndSubCategory2Async(int categoryId, int subCategory1Id, int subCategory2Id)
        {
            return await _context.DowntimeSubCategory2
                .Include(sc2 => sc2.DowntimeSubCategory1)
                .Where(sc2 => sc2.DowntimeSubCategory1.DowntimeCategoryID == categoryId && sc2.DowntimeSubCategory1ID == subCategory1Id && sc2.Id == subCategory2Id)
                .Select(sc2 => new SubCategory2WithSubCategory1DTO
                {
                    SubCategory2Id = sc2.Id,
                    SubCategory2 = sc2.Name,
                    SubCategory1 = sc2.DowntimeSubCategory1.Name,
                    SubCategory1Id = sc2.DowntimeSubCategory1ID,
                    Category = sc2.DowntimeSubCategory1.DowntimeCategory.Name,
                    CategoryId = sc2.DowntimeSubCategory1.DowntimeCategoryID,
                    Inactive = sc2.Inactive
                })
                .ToListAsync();
        }

        // GET: api/DowntimeSubCategory2/select
        [HttpGet("select/{subCategory1Id}")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetDowntimeSubCategory2Select(int subCategory1Id)
        {
            if (_context.DowntimeSubCategory2 == null)
            {
                return NotFound();
            }

            var subCategories2 = await _context.DowntimeSubCategory2
                .Include(sc2 => sc2.DowntimeSubCategory1)
                    .ThenInclude(sc1 => sc1.DowntimeCategory)
                .Where(sc2 => sc2.DowntimeSubCategory1ID == subCategory1Id)
                .Select(sc2 => new DTOReactDropdown<int>
                {
                    Label = sc2.Name,
                    Value = sc2.Id
                })
                .ToListAsync();

            return Ok(subCategories2);
        }

        // GET: api/DowntimeSubCategory2/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DowntimeSubCategory2>> GetDowntimeSubCategory2(int id)
        {
            if (_context.DowntimeSubCategory2 == null)
            {
                return NotFound();
            }
            var downtimeSubCategory2 = await _context.DowntimeSubCategory2.FindAsync(id);

            if (downtimeSubCategory2 == null)
            {
                return NotFound();
            }

            return downtimeSubCategory2;
        }

        // PUT: api/DowntimeSubCategory2/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDowntimeSubCategory2(int id, DowntimeSubCategory2 downtimeSubCategory2)
        {
            if (id != downtimeSubCategory2.Id)
            {
                return BadRequest();
            }

            _context.Entry(downtimeSubCategory2).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DowntimeSubCategory2Exists(id))
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
            var category = await _context.DowntimeSubCategory2.FindAsync(id);

            if (category != null)
            {
                category.Inactive = !category.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/DowntimeSubCategory2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DowntimeSubCategory2>> PostDowntimeSubCategory2(DowntimeSubCategory2 downtimeSubCategory2)
        {
            if (_context.DowntimeSubCategory2 == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DowntimeSubCategory2'  is null.");
            }
            _context.DowntimeSubCategory2.Add(downtimeSubCategory2);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDowntimeSubCategory2", new { id = downtimeSubCategory2.Id }, downtimeSubCategory2);
        }

        // DELETE: api/DowntimeSubCategory2/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDowntimeSubCategory2(int id)
        {
            var downtimeSubCategory2 = await _context.DowntimeSubCategory2.FindAsync(id);
            if (downtimeSubCategory2 == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    IQueryable<DowntimeReason> reasons = _context.DowntimeReason;
                    IQueryable<DowntimeCode> codes = _context.DowntimeCode;

                    codes = codes.Where(c => c.DowntimeSubCategory2ID == downtimeSubCategory2.Id);
                    List<DowntimeCode> listDeleteCodes = codes.ToList();

                    reasons = reasons.Where(r => r.DowntimeSubCategory2Id == downtimeSubCategory2.Id);
                    List<DowntimeReason> listReason = reasons.ToList();
                    List<int> productivityIds = listReason.Select(r => r.ProductivityID).ToList();
                    List<Productivity> listDelete = query.Where(p => productivityIds.Contains(p.Id)).ToList();


                    _context.DowntimeCode.RemoveRange(listDeleteCodes);
                    _context.Productivity.RemoveRange(listDelete);
                    _context.DowntimeSubCategory2.Remove(downtimeSubCategory2);
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

        private bool DowntimeSubCategory2Exists(int id)
        {
            return (_context.DowntimeSubCategory2?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
