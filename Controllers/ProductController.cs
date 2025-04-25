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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ProductController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            if (_context.Product == null)
            {
                return NotFound();
            }

            var productList = await _context.Product
            .Include(e => e.Line)
            .ToListAsync();

            return productList;

            // return await _context.Product.ToListAsync();
        }

        public struct ProductFilterData
        {
            public string[] NetContent { get; set; }
            public string[] Packing { get; set; }
            public string[] Flavour { get; set; }
            public int[] UnitsPerPackage { get; set; }
            public int[] StandardSpeed { get; set; }

            public List<Line> Lines { get; set; }

        }
        // GET: api/Product/GetProductFiltersData
        [HttpGet("ProductFiltersData")]
        public async Task<ActionResult<IEnumerable<ProductFilterData>>> GetProductFiltersData()
        {
            if (_context.Product == null)
            {
                return NotFound();
            }

            var productList = await _context.Product
            .Include(e => e.Line)
            .ToListAsync();

            var lineList = await _context.Line
            .ToListAsync();

            ProductFilterData results = new()
            {
                NetContent = productList.OrderBy(s => s.NetContent).Select(p => p.NetContent).Distinct().ToArray<string>(),
                Packing = productList.OrderBy(s => s.Packing).Select(p => p.Packing).Distinct().ToArray<string>(),
                Flavour = productList.OrderBy(s => s.Flavour).Select(p => p.Flavour).Distinct().ToArray<string>(),
                UnitsPerPackage = productList.OrderBy(s => s.UnitsPerPackage).Select(p => p.UnitsPerPackage).Distinct().ToArray<int>(),
                StandardSpeed = productList.OrderBy(s => s.StandardSpeed).Select(p => p.StandardSpeed).Distinct().ToArray<int>(),
                Lines = lineList,
            };

            return Ok(results);

            // return await _context.Product.ToListAsync();
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            if (_context.Product == null)
            {
                return NotFound();
            }
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Product/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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
            var product = await _context.Product.FindAsync(id);

            if (product != null)
            {
                product.Inactive = !product.Inactive;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // POST: api/Product
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Product'  is null.");
            }
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IQueryable<Productivity> query = _context.Productivity;
                    query = query.Where(q => q.LineID == product.LineID);
                    query = query.Where(q => q.ProductID == product.Id);
                    query = query.Where(q => q.Sku == product.Sku);
                    List<Productivity> listDelete = query.ToList();

                    _context.Productivity.RemoveRange(listDelete);
                    _context.Product.Remove(product);
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

        public struct ProductFilteredData
        {
            public string? NetContent { get; set; }
            public string? Packing { get; set; }
            public string? Flavour { get; set; }
            public int? UnitsPerPackage { get; set; }
            public int? StandardSpeed { get; set; }
            public int? LineId { get; set; }
            public string? SearchTerm { get; set; }

        }
        // GET: api/Product/Filtered
        [HttpPost("Filtered")]
        public async Task<IActionResult> GetProductsFiltered([FromBody] ProductFilteredData filters)
        {
            var query = _context.Product.Include(e => e.Line).AsQueryable();

            if (!string.IsNullOrEmpty(filters.NetContent))
                query = query.Where(p => p.NetContent == filters.NetContent);

            if (!string.IsNullOrEmpty(filters.Packing))
                query = query.Where(p => p.Packing == filters.Packing);

            if (!string.IsNullOrEmpty(filters.Flavour))
                query = query.Where(p => p.Flavour == filters.Flavour);

            if (filters.UnitsPerPackage.HasValue)
                query = query.Where(p => p.UnitsPerPackage == filters.UnitsPerPackage.Value);

            if (filters.StandardSpeed.HasValue)
                query = query.Where(p => p.StandardSpeed == filters.StandardSpeed.Value);

            if (filters.LineId.HasValue)
                query = query.Where(p => p.LineID == filters.LineId);

            var filteredList = await query.ToListAsync();
            return Ok(filteredList);
        }

        // GET: api/Product/excel-list
        [HttpPost("excel-list")]
        public async Task<IActionResult> GenerateExcel([FromBody] ProductFilteredData filters)
        {
            try
            {
                var query = _context.Product.Include(e => e.Line).AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(filters.NetContent))
                    query = query.Where(p => p.NetContent == filters.NetContent);

                if (!string.IsNullOrEmpty(filters.Packing))
                    query = query.Where(p => p.Packing == filters.Packing);

                if (!string.IsNullOrEmpty(filters.Flavour))
                    query = query.Where(p => p.Flavour == filters.Flavour);

                if (filters.UnitsPerPackage.HasValue)
                    query = query.Where(p => p.UnitsPerPackage == filters.UnitsPerPackage.Value);

                if (filters.StandardSpeed.HasValue)
                    query = query.Where(p => p.StandardSpeed == filters.StandardSpeed.Value);

                if (filters.LineId.HasValue)
                    query = query.Where(p => p.LineID == filters.LineId);

                if (!string.IsNullOrEmpty(filters.SearchTerm))
                {
                    string searchTerm = filters.SearchTerm.Trim().ToLower();
                    query = query.Where(p =>
                        p.Sku.ToLower().Contains(searchTerm) ||
                        p.NetContent.ToLower().Contains(searchTerm) ||
                        p.Packing.ToLower().Contains(searchTerm) ||
                        p.Flavour.ToLower().Contains(searchTerm) ||
                        (p.Line != null && p.Line.Name.ToLower().Contains(searchTerm))
                    );
                }

                var filteredList = await query.ToListAsync();

                DataTable dt = new("Products");

                dt.Columns.AddRange(new DataColumn[] {
                    new("SKU"),
                    new("Net Content"),
                    new("Packing"),
                    new("Flavour"),
                    new("Line"),
                    new("Units Per Package"),
                    new("Standard Speed"),
                    new("Inactive")
                });

                foreach (var product in filteredList)
                {
                    dt.Rows.Add(
                        product.Sku,
                        product.NetContent,
                        product.Packing,
                        product.Flavour,
                        product.Line?.Name,
                        product.UnitsPerPackage,
                        product.StandardSpeed,
                        product.Inactive ? "Yes" : "No"
                    );
                }

                using XLWorkbook wb = new();
                wb.Worksheets.Add(dt);
                using MemoryStream stream = new();
                wb.SaveAs(stream);
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "ProductReport.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }
        private bool ProductExists(int id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
