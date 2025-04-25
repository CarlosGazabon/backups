using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Inventio.Data;
using Inventio.Models;

using Microsoft.AspNetCore.JsonPatch;
using inventio.Repositories.Dashboards.NonConformanceAnalysis;
using inventio.Models.DTO.NonConformance;
using inventio.Models.DTO.QualityIncident;
using Hangfire;
using Inventio.Jobs.NonConformance;

namespace Inventio.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class QualityIncidentController : ControllerBase
    {

        private readonly INonConformanceAnalysisRepository _nonConformanceRepository;
        private readonly ApplicationDBContext _context;

        public QualityIncidentController(ApplicationDBContext context, INonConformanceAnalysisRepository nonConformanceRepository)
        {
            _nonConformanceRepository = nonConformanceRepository;
            _context = context;

        }

        //* --------------------------------- Filters -------------------------------- */        
        [HttpPost("lines")]
        public async Task<ActionResult> AddLines([FromBody] NonConformanceFilter filter)
        {
            try
            {
                var lines = await _nonConformanceRepository.GetLines(filter);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        //* -------------------------------- Dashboard ------------------------------- */        
        [HttpPost("dashboard")]
        public async Task<IActionResult> AddDashboard([FromBody] NonConformanceFilter filter)
        {
            try
            {
                var result = await _nonConformanceRepository.GetDashboard(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor: " + ex.Message });
            }
        }

        // GET: api/QualityIncident
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QualityIncident>>> GetQualityIncident()
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }
            return await _context.QualityIncident.ToListAsync();
        }

        // GET: api/QualityIncident/paginated
        [HttpGet("paginated")]
        public async Task<ActionResult> GetQualityIncidentPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }

            var totalRecords = await _context.QualityIncident.CountAsync();
            var paginatedResult = await _context.QualityIncident
                .Include(q => q.Line)
                .Include(q => q.Shift)
                .Include(q => q.Product)
                .Include(q => q.QualityIncidentReason)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { TotalRecords = totalRecords, Records = paginatedResult });
        }


        // GET: api/QualityIncident/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportQualityIncidentsToExcel()
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }

            var qualityIncidents = await _context.QualityIncident
            .Include(q => q.Line)
            .Include(q => q.Shift)
            .Include(q => q.Product)
            .Include(q => q.QualityIncidentReason)
            .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("QualityIncidents");
                var currentRow = 1;

                // Header                                
                worksheet.Cell(currentRow, 1).Value = "#Hold";
                worksheet.Cell(currentRow, 2).Value = "Date";
                worksheet.Cell(currentRow, 3).Value = "Batch Number";
                worksheet.Cell(currentRow, 4).Value = "Expiration Code";
                worksheet.Cell(currentRow, 5).Value = "Product Description";
                worksheet.Cell(currentRow, 6).Value = "SKU";
                worksheet.Cell(currentRow, 7).Value = "Line";
                worksheet.Cell(currentRow, 8).Value = "Shift";
                worksheet.Cell(currentRow, 9).Value = "Quantity (Cases)";
                worksheet.Cell(currentRow, 10).Value = "Hold Reason";
                worksheet.Cell(currentRow, 11).Value = "Next Steps";
                worksheet.Cell(currentRow, 12).Value = "Comments";
                worksheet.Cell(currentRow, 13).Value = "Potential Root Cause";
                worksheet.Cell(currentRow, 14).Value = "Status";
                worksheet.Cell(currentRow, 15).Value = "Released By";
                worksheet.Cell(currentRow, 16).Value = "Released Date";

                // Body
                foreach (var incident in qualityIncidents)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = incident.IncidentNumber;
                    worksheet.Cell(currentRow, 2).Value = incident.DateOfIncident.ToString();
                    worksheet.Cell(currentRow, 3).Value = incident.BatchNumber;
                    worksheet.Cell(currentRow, 4).Value = incident.ExpirationCode;
                    worksheet.Cell(currentRow, 5).Value = incident.Product?.Flavour + incident.Product?.NetContent;
                    worksheet.Cell(currentRow, 6).Value = incident.Product?.Sku;
                    worksheet.Cell(currentRow, 7).Value = incident.Line?.Name;
                    worksheet.Cell(currentRow, 8).Value = incident.Shift?.Name;
                    worksheet.Cell(currentRow, 9).Value = incident.Quantity;
                    worksheet.Cell(currentRow, 10).Value = incident.QualityIncidentReason?.Description;
                    worksheet.Cell(currentRow, 11).Value = incident.ActionComments;
                    worksheet.Cell(currentRow, 12).Value = incident.Comments;
                    worksheet.Cell(currentRow, 13).Value = incident.PotencialRootCause;
                    worksheet.Cell(currentRow, 14).Value = "";
                    worksheet.Cell(currentRow, 15).Value = "";
                    worksheet.Cell(currentRow, 16).Value = "";

                }

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "QualityIncidents.xlsx");
                }
            }
        }

        private IQueryable<QualityIncident> BuildQualityIncidentQuery(DateOnly? startDate, DateOnly? endDate, string? sku, int? line, int? holdReason, string? status, Boolean holds)
        {
            var query = _context.QualityIncident.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(q => q.DateOfIncident >= startDate.Value && q.DateOfIncident <= endDate.Value);
            }
            else if (startDate.HasValue)
            {
                query = query.Where(q => q.DateOfIncident >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                query = query.Where(q => q.DateOfIncident <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(sku))
            {
                var productIds = _context.Product
                    .Where(p => p.Sku.Contains(sku))
                    .Select(p => p.Id)
                    .ToList();

                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            if (line.HasValue)
            {
                query = query.Where(q => q.LineId == line.Value);
            }

            if (holdReason.HasValue)
            {
                query = query.Where(q => q.QualityIncidentReasonId == holdReason.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "hold")
                {
                    query = query.Where(q => q.Released == false);
                }
                else if (status == "released")
                {
                    query = query.Where(q => q.Released == true);
                }
            }

            if (holds)
            {
                query = query.Where(q => q.Released == false);
            }

            return query;
        }

        // GET: api/QualityIncident/filter
        [HttpGet("filter")]
        public async Task<ActionResult> GetQualityIncidentFiltered(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? sku,
            [FromQuery] int? line,
            [FromQuery] int? holdReason,
            [FromQuery] string? status,
            [FromQuery] bool holds,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }

            var query = BuildQualityIncidentQuery(startDate, endDate, sku, line, holdReason, status, holds);

            var totalRecords = await query.CountAsync();
            var paginatedResult = await query
                .Include(q => q.AuditCreatedBy)
                .Include(q => q.AuditUpdatedBy)
                .Include(q => q.ReleasedBy)
                .Include(q => q.Line)
                .Include(q => q.Shift)
                .Include(q => q.Product)
                .Include(q => q.QualityIncidentReason)
                .OrderByDescending(q => q.AuditDateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { TotalRecords = totalRecords, Records = paginatedResult });
        }

        // GET: api/QualityIncident/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QualityIncident>> GetQualityIncident(int id)
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }
            var qualityIncident = await _context.QualityIncident
                .Include(q => q.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            // var qualityIncident = await _context.QualityIncident.FindAsync(id);

            if (qualityIncident == null)
            {
                return NotFound();
            }

            return qualityIncident;
        }

        // PUT: api/QualityIncident/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQualityIncident(int id, QualityIncidentEditDTO qualityIncidentDTO)
        {
            var qualityIncident = await _context.QualityIncident.FindAsync(id);

            if (qualityIncident == null)
            {
                return NotFound();
            }

            qualityIncident.DateOfIncident = qualityIncidentDTO.DateOfIncident;
            qualityIncident.IncidentNumberExtra = qualityIncidentDTO.IncidentNumberExtra;
            qualityIncident.LineId = qualityIncidentDTO.LineId;
            qualityIncident.ShiftId = qualityIncidentDTO.ShiftId;
            qualityIncident.BatchNumber = qualityIncidentDTO.BatchNumber;
            qualityIncident.ExpirationCode = qualityIncidentDTO.ExpirationCode;
            qualityIncident.ProductId = qualityIncidentDTO.ProductId;
            qualityIncident.Quantity = qualityIncidentDTO.Quantity;
            qualityIncident.QualityIncidentReasonId = qualityIncidentDTO.QualityIncidentReasonId;
            qualityIncident.ActionComments = qualityIncidentDTO.ActionComments;
            qualityIncident.PotencialRootCause = qualityIncidentDTO.PotencialRootCause;
            qualityIncident.PalletsCodeNumber = qualityIncidentDTO.PalletsCodeNumber;
            qualityIncident.Comments = qualityIncidentDTO.Comments;

            qualityIncident.AuditUpdatedById = qualityIncidentDTO.EditById;
            qualityIncident.AuditDateUpdated = DateTime.UtcNow;


            _context.Entry(qualityIncident).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QualityIncidentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            BackgroundJob.Enqueue<EditIncidentEventJob>(x => x.ExecuteAsync(id));

            return NoContent();
        }

        // PUT: api/QualityIncident/release/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("release/{id}")]
        public async Task<IActionResult> PutQualityIncidentRelease(int id, QualityIncidentReleaseDTO qualityIncidentDTO)
        {
            var qualityIncident = await _context.QualityIncident.FindAsync(id);

            if (qualityIncident == null)
            {
                return NotFound();
            }

            qualityIncident.ReleasedForConsumption = qualityIncidentDTO.ReleasedForConsumption;
            qualityIncident.ReleasedForDonation = qualityIncidentDTO.ReleasedForDonation;
            qualityIncident.ReleasedForDestruction = qualityIncidentDTO.ReleasedForDestruction;
            qualityIncident.ReleasedForOther = qualityIncidentDTO.ReleasedForOther;
            qualityIncident.ReleaseComments = qualityIncidentDTO.ReleaseComments;

            qualityIncident.ReleasedById = qualityIncidentDTO.ReleasedById;
            qualityIncident.DateOfRelease = DateOnly.FromDateTime(DateTime.UtcNow);

            qualityIncident.Released = true;
            qualityIncident.AuditUpdatedById = qualityIncidentDTO.ReleasedById;
            qualityIncident.AuditDateUpdated = DateTime.UtcNow;

            _context.Entry(qualityIncident).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QualityIncidentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            BackgroundJob.Enqueue<ReleaseIncidentEventJob>(x => x.ExecuteAsync(id));

            return NoContent();
        }

        // POST: api/QualityIncident
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        // public async Task<ActionResult<QualityIncident>> PostQualityIncident(QualityIncident qualityIncident)

        public async Task<ActionResult<QualityIncident>> PostQualityIncident(QualityIncidentDTO qualityIncident)
        {
            if (_context.QualityIncident == null)
            {
                return Problem("Entity set 'ApplicationDBContext.QualityIncident' is null.");
            }

            var newIncident = new QualityIncident()
            {
                IncidentNumber = qualityIncident.IncidentNumber,
                IncidentNumberExtra = qualityIncident.IncidentNumberExtra,
                DateOfIncident = qualityIncident.DateOfIncident,
                BatchNumber = qualityIncident.BatchNumber,
                ExpirationCode = qualityIncident.ExpirationCode,
                ProductId = qualityIncident.ProductId,
                LineId = qualityIncident.LineId,
                ShiftId = qualityIncident.ShiftId,
                QualityIncidentReasonId = qualityIncident.QualityIncidentReasonId,
                Quantity = qualityIncident.Quantity,
                ActionComments = qualityIncident.ActionComments,
                PalletsCodeNumber = qualityIncident.PalletsCodeNumber,
                Comments = qualityIncident.Comments,
                PotencialRootCause = qualityIncident.PotencialRootCause,
                AuditDateCreated = DateTime.UtcNow,
                AuditDateUpdated = DateTime.UtcNow,
                AuditCreatedById = qualityIncident.createdById,
                AuditUpdatedById = qualityIncident.createdById
            };

            _context.QualityIncident.Add(newIncident);

            await _context.SaveChangesAsync();

            BackgroundJob.Enqueue<NewIncidentEventJob>(x => x.ExecuteAsync(newIncident.Id));

            return CreatedAtAction("GetQualityIncident", new { id = newIncident.Id }, newIncident);
        }

        // DELETE: api/QualityIncident/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQualityIncident(int id)
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }
            var qualityIncident = await _context.QualityIncident.FindAsync(id);
            if (qualityIncident == null)
            {
                return NotFound();
            }

            _context.QualityIncident.Remove(qualityIncident);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QualityIncidentExists(int id)
        {
            return (_context.QualityIncident?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: api/QualityIncident/consecutive
        [HttpGet("consecutive")]
        public async Task<ActionResult> GetConsecutiveIncidentCode()
        {
            if (_context.QualityIncident == null)
            {
                return NotFound();
            }

            var currentYearLastTwoDigits = DateTime.Now.Year % 100;

            var lastIncident = await _context.QualityIncident
            .OrderByDescending(q => q.Id)
            .FirstOrDefaultAsync();

            if (lastIncident == null)
            {
                return Ok(new { LastCode = "", SuggestedCode = $"{currentYearLastTwoDigits}-001" });
            }

            var lastCode = lastIncident.IncidentNumber;
            var numericPart = int.Parse(lastCode.Substring(3));
            var newCode = $"{currentYearLastTwoDigits}-{(numericPart + 1).ToString("D3")}";

            return Ok(new { LastCode = lastCode, SuggestedCode = newCode });
        }
    }
}
