using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using inventio.Models.DTO;
using inventio.Repositories.Settings.SubCategory1;
using inventio.Models.DTO.Settings.SubCategory1;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeSubCategory1Controller : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ISubCategory1Repository _subCategory1Repository;

        public DowntimeSubCategory1Controller(ApplicationDBContext context, ISubCategory1Repository subCategory1Repository)
        {
            _context = context;
            _subCategory1Repository = subCategory1Repository;
        }

        // GET: api/DowntimeSubCategory1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeSubCategory1>>> GetDowntimeSubCategory1()
        {
            if (_context.DowntimeSubCategory1 == null)
            {
                return NotFound();
            }
            return await _context.DowntimeSubCategory1.Include(e => e.DowntimeCategory).ToListAsync();
        }
        [HttpGet("table")]
        public async Task<ActionResult<IEnumerable<SubCategoryWithCategoryDTO>>> GetDowntimeSubCategory1Table()
        {
            if (_context.DowntimeSubCategory1 == null)
            {
                return NotFound();
            }

            var subCategories = await _context.DowntimeSubCategory1
                .Include(e => e.DowntimeCategory)
                .Select(sc => new SubCategoryWithCategoryDTO
                {
                    SubCategory1Id = sc.Id,
                    SubCategory1 = sc.Name,
                    Category = sc.DowntimeCategory.Name,
                    Inactive = sc.Inactive
                })
                .ToListAsync();

            return Ok(subCategories);
        }

        // GET: api/DowntimeSubCategory1/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DowntimeSubCategory1>> GetDowntimeSubCategory1(int id)
        {
            if (_context.DowntimeSubCategory1 == null)
            {
                return NotFound();
            }
            var downtimeSubCategory1 = await _context.DowntimeSubCategory1.FindAsync(id);

            if (downtimeSubCategory1 == null)
            {
                return NotFound();
            }

            return downtimeSubCategory1;
        }

        // GET: api/DowntimeSubCategory1/category/select/5
        [HttpGet("category/select/{categoryId}")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetDowntimeSubCategory1ByCategory(int categoryId)
        {
            var subCategories = await _subCategory1Repository.GetDowntimeSubCategoriesByCategoryAsync(categoryId);

            if (subCategories == null || !subCategories.Any())
            {
                return NotFound();
            }

            return Ok(subCategories);
        }

        // GET: api/DowntimeSubCategory1/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<SubCategoryWithCategoryDTO>>> GetSubCategoriesWithCategory(int categoryId)
        {
            var subCategories = await _subCategory1Repository.GetSubCategoriesWithCategoryAsync(categoryId);

            if (subCategories == null || !subCategories.Any())
            {
                return NotFound();
            }

            return Ok(subCategories);
        }

        // GET: api/DowntimeSubCategory1/category/5/subcategory/2
        [HttpGet("category/{categoryId}/subcategory/{subCategoryId}")]
        public async Task<ActionResult<IEnumerable<SubCategoryWithCategoryDTO>>> GetSubCategoriesWithCategoryAndSubCategory(int categoryId, int subCategoryId)
        {
            var subCategories = await _subCategory1Repository.GetSubCategoriesWithCategoryAndSubCategoryAsync(categoryId, subCategoryId);

            if (subCategories == null || !subCategories.Any())
            {
                return NotFound();
            }

            return Ok(subCategories);
        }


        // PUT: api/DowntimeSubCategory1/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDowntimeSubCategory1(int id, DowntimeSubCategory1 downtimeSubCategory1)
        {
            if (id != downtimeSubCategory1.Id)
            {
                return BadRequest();
            }

            _context.Entry(downtimeSubCategory1).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DowntimeSubCategory1Exists(id))
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
            var category = await _context.DowntimeSubCategory1.FindAsync(id);

            if (category != null)
            {
                category.Inactive = !category.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/DowntimeSubCategory1
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DowntimeSubCategory1>> PostDowntimeSubCategory1(DowntimeSubCategory1 downtimeSubCategory1)
        {
            if (_context.DowntimeSubCategory1 == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DowntimeSubCategory1'  is null.");
            }
            _context.DowntimeSubCategory1.Add(downtimeSubCategory1);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDowntimeSubCategory1", new { id = downtimeSubCategory1.Id }, downtimeSubCategory1);
        }

        // DELETE: api/DowntimeSubCategory1/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDowntimeSubCategory1(int id)
        {
            var downtimeSubCategory1 = await _context.DowntimeSubCategory1.FindAsync(id);
            if (downtimeSubCategory1 == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    IQueryable<DowntimeReason> reasons = _context.DowntimeReason;
                    IQueryable<DowntimeSubCategory2> categorie2 = _context.DowntimeSubCategory2;
                    IQueryable<DowntimeCode> codes = _context.DowntimeCode;

                    codes = codes.Where(code => code.DowntimeSubCategory1ID == downtimeSubCategory1.Id);
                    List<DowntimeCode> listDeleteCodes = codes.ToList();

                    categorie2 = categorie2.Where(cat => cat.DowntimeSubCategory1ID == downtimeSubCategory1.Id);
                    List<DowntimeSubCategory2> listDeleteCat2 = categorie2.ToList();

                    reasons = reasons.Where(r => r.DowntimeSubCategory1Id == downtimeSubCategory1.Id);
                    List<DowntimeReason> listReason = reasons.ToList();
                    List<int> productivityIds = listReason.Select(r => r.ProductivityID).ToList();
                    List<Productivity> listDelete = query.Where(p => productivityIds.Contains(p.Id)).ToList();


                    _context.DowntimeCode.RemoveRange(listDeleteCodes);
                    _context.DowntimeSubCategory2.RemoveRange(listDeleteCat2);
                    _context.Productivity.RemoveRange(listDelete);
                    _context.DowntimeSubCategory1.Remove(downtimeSubCategory1);
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



        private bool DowntimeSubCategory1Exists(int id)
        {
            return (_context.DowntimeSubCategory1?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
