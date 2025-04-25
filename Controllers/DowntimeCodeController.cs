using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using System.Data;
using ClosedXML.Excel;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeCodeController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DowntimeCodeController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/DowntimeCode
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeCode>>> GetDowntimeCode()
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).OrderBy(e => e.DowntimeSubCategory2ID).ThenBy(e => e.Code).ToListAsync();
        }

        // GET: api/DowntimeCode/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<DowntimeCode>>> GetDowntimeCodeActive()
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).Where(x => !x.Inactive).OrderBy(x => x.Code).ToListAsync();
        }

        // GET: api/DowntimeCode/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DowntimeCode>> GetDowntimeCode(int id)
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            var downtimeCode = await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).FirstOrDefaultAsync(i => i.Id == id);

            if (downtimeCode == null)
            {
                return NotFound();
            }

            return downtimeCode;
        }

        // GET: api/DowntimeCode/CheckCodeExists/x-xx-xxx
        [HttpGet("CheckCodeExists/{code}")]
        public async Task<IActionResult> GetCheckCodeExists(string code)
        {
            System.Diagnostics.Debug.WriteLine(code);
            // Check if the model exists.
            bool exists = await _context.DowntimeCode.AnyAsync(m => m.Code == code);

            // Return the result.
            if (exists)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        // PUT: api/DowntimeCode/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDowntimeCode(int id, DowntimeCode downtimeCode)
        {
            if (id != downtimeCode.Id)
            {
                return BadRequest();
            }

            _context.Entry(downtimeCode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DowntimeCodeExists(id))
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
            var downtimeCode = await _context.DowntimeCode.FindAsync(id);

            if (downtimeCode != null)
            {
                downtimeCode.Inactive = !downtimeCode.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/DowntimeCode
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DowntimeCode>> PostDowntimeCode(DowntimeCode downtimeCode)
        {
            if (_context.DowntimeCode == null)
            {
                return Problem("Entity set 'ApplicationDBContext.DowntimeCode'  is null.");
            }
            _context.DowntimeCode.Add(downtimeCode);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDowntimeCode", new { id = downtimeCode.Id }, downtimeCode);
        }

        // DELETE: api/DowntimeCode/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDowntimeCode(int id)
        {
            var downtimeCode = await _context.DowntimeCode.FindAsync(id);
            if (downtimeCode == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    IQueryable<DowntimeReason> reasons = _context.DowntimeReason;

                    reasons = reasons.Where(r => r.DowntimeCodeId == downtimeCode.Id);
                    List<DowntimeReason> listReason = reasons.ToList();
                    List<int> productivityIds = listReason.Select(r => r.ProductivityID).ToList();
                    List<Productivity> listDelete = query.Where(p => productivityIds.Contains(p.Id)).ToList();
                    _context.Productivity.RemoveRange(listDelete);
                    _context.DowntimeCode.Remove(downtimeCode);
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

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<DowntimeCategory>>> GetDowntimeCategories()
        {
            if (_context.DowntimeCategory == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCategory.OrderBy(x => x.Name).ToListAsync();
        }


        // GET: api/DowntimeCode/category/1
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<DowntimeCode>>> GetDowntimeCodesByCategory(int categoryId)
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).Where(x => x.DowntimeCategoryID == categoryId).OrderBy(x => x.Code).ToListAsync();
        }

        // GET: api/DowntimeCode/category/5/subcategory/2
        [HttpGet("category/{categoryId}/subcategory/{subCategoryId}")]
        public async Task<ActionResult<IEnumerable<DowntimeCode>>> GetDowntimeCodesByCategoryAndSubCategory(int categoryId, int subCategoryId)
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).Where(x => x.DowntimeCategoryID == categoryId && x.DowntimeSubCategory1ID == subCategoryId).OrderBy(x => x.Code).ToListAsync();
        }

        // GET: api/DowntimeCode/category/5/subcategory1/2/subcategory2/3
        [HttpGet("category/{categoryId}/subcategory1/{subCategoryId1}/subcategory2/{subCategoryId2}")]
        public async Task<ActionResult<IEnumerable<DowntimeCode>>> GetDowntimeCodesByCategoryAndSubCategory1AndSubCategory2(int categoryId, int subCategoryId1, int subCategoryId2)
        {
            if (_context.DowntimeCode == null)
            {
                return NotFound();
            }
            return await _context.DowntimeCode.Include(c => c.DowntimeCategory).Include(c => c.DowntimeSubCategory1).Include(c => c.DowntimeSubCategory2).Where(x => x.DowntimeCategoryID == categoryId && x.DowntimeSubCategory1ID == subCategoryId1 && x.DowntimeSubCategory2ID == subCategoryId2).OrderBy(x => x.Code).ToListAsync();
        }

        public class DowntimeCodeFilter
        {
            public int? CategoryId { get; set; }
            public int? SubCategory1Id { get; set; }
            public int? SubCategory2Id { get; set; }
            public string? SearchTerm { get; set; }
        }
        //GET: api/DowntimeCode/excel
        [HttpPost("excel")]
        public async Task<IActionResult> GenerateExcel([FromBody] DowntimeCodeFilter filters)
        {
            try
            {
                var query = _context.DowntimeCode
                                    .Include(c => c.DowntimeCategory)
                                    .Include(c => c.DowntimeSubCategory1)
                                    .Include(c => c.DowntimeSubCategory2)
                                    .AsQueryable();

                if (filters.CategoryId.HasValue && filters.CategoryId.Value != 0)
                    query = query.Where(dc => dc.DowntimeCategoryID == filters.CategoryId.Value);

                if (filters.SubCategory1Id.HasValue && filters.SubCategory1Id.Value != 0)
                    query = query.Where(dc => dc.DowntimeSubCategory1ID == filters.SubCategory1Id.Value);

                if (filters.SubCategory2Id.HasValue && filters.SubCategory2Id.Value != 0)
                    query = query.Where(dc => dc.DowntimeSubCategory2ID == filters.SubCategory2Id.Value);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    string searchTerm = filters.SearchTerm.Trim().ToLower();
                    query = query.Where(dc =>
                        dc.Code.ToLower().Contains(searchTerm) ||
                        dc.Failure.ToLower().Contains(searchTerm) ||
                        (dc.DowntimeCategory != null && dc.DowntimeCategory.Name.ToLower().Contains(searchTerm)) ||
                        (dc.DowntimeSubCategory1 != null && dc.DowntimeSubCategory1.Name.ToLower().Contains(searchTerm)) ||
                        (dc.DowntimeSubCategory2 != null && dc.DowntimeSubCategory2.Name.ToLower().Contains(searchTerm))
                    );
                }
                var filteredList = await query.ToListAsync();

                DataTable dt = new("DowntimeCodes");

                dt.Columns.AddRange(new DataColumn[] {
                    new ("Downtime Category"),
                    new ("Downtime SubCategory 1"),
                    new ("Downtime SubCategory 2"),
                    new ("Code"),
                    new ("Failure"),
                    new ("Inactive")
                });

                foreach (var downtimeCode in filteredList)
                {
                    dt.Rows.Add(
                        downtimeCode.DowntimeCategory?.Name,
                        downtimeCode.DowntimeSubCategory1?.Name,
                        downtimeCode.DowntimeSubCategory2?.Name,
                        downtimeCode.Code,
                        downtimeCode.Failure,
                        downtimeCode.Inactive ? "Yes" : "No"
                    );
                }

                using XLWorkbook wb = new();
                wb.Worksheets.Add(dt);
                using MemoryStream stream = new();
                wb.SaveAs(stream);
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "DowntimeCodesReport.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        private bool DowntimeCodeExists(int id)
        {
            return (_context.DowntimeCode?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
