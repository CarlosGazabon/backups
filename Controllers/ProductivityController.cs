// using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using inventio.Models.DTO;
using Inventio.Models.Views;
using inventio.Models.DTO.Productivity;
using System.Globalization;
using System.Data;
using ClosedXML.Excel;
using Inventio.Models.DTO.Productivity;
using inventio.Repositories.Records.Production;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductivityController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IProductionRepository _productionRepository;

        public ProductivityController(ApplicationDBContext context, IProductionRepository productionRepository)
        {
            _context = context;
            _productionRepository = productionRepository;
        }

        public static decimal getAverage(decimal total, decimal part)
        {
            if (total == 0)
            {
                return 0;
            }
            return Math.Round((part / total) * 100, 2, MidpointRounding.AwayFromZero);
        }
        public struct TableDataRow
        {
            public string Line { get; set; }
            public decimal Average { get; set; }
            public decimal Production { get; set; }
        }
        public struct ScrapTableDataRow
        {
            public string Line { get; set; }
            public decimal Average { get; set; }
            public decimal ScrapUnits { get; set; }
        }

        public struct EfficiencyTableRow
        {
            public string Line { get; set; }
            public decimal EfficiencyCal { get; set; }
            public decimal MinOfEfficiency { get; set; }

        }
        public struct SummaryReportBag
        {
            public IEnumerable<Summary> summaryList { get; set; }
            public int totalProduction { get; set; }
            public int totalScrapUnits { get; set; }

            public decimal efficiencyTableTotalAverage { get; set; }

            public IEnumerable<TableDataRow> productionTableList { get; set; }
            public IEnumerable<ScrapTableDataRow> scrapTableList { get; set; }
            public IEnumerable<EfficiencyTableRow> efficiencyTable { get; set; }

        }

        // GET: api/Summary
        [HttpGet("Summary")]
        public async Task<ActionResult<SummaryReportBag>> GetSummary()
        {
            if (_context.Summary == null)
            {
                return NotFound();
            }

            var summaryList = await _context.Summary.ToListAsync();

            var totalProduction = summaryList.Aggregate(0, (acc, x) => acc + Decimal.ToInt32(x.Production));
            var totalScrapUnits = summaryList.Aggregate(0, (acc, x) => acc + Decimal.ToInt32(x.ScrapUnits));


            var baseList = await _context.Summary.ToListAsync();

            var productionTableList = baseList.GroupBy(x => x.Line).Select(g => new TableDataRow() { Line = g.Key, Production = g.Sum(x => x.Production), Average = getAverage(totalProduction, g.Sum(x => x.Production)) }).ToList();


            var scrapTableList = baseList.GroupBy(x => x.Line).Select(g => new ScrapTableDataRow() { Line = g.Key, ScrapUnits = g.Sum(x => x.ScrapUnits), Average = getAverage(totalScrapUnits, g.Sum(x => x.ScrapUnits)) }).ToList();

            var efficiencyTable = baseList.GroupBy(x => x.Line).Select(g => new EfficiencyTableRow() { Line = g.Key, EfficiencyCal = Math.Round(g.Sum(x => x.Production) / g.Sum(x => x.EffDEN), 2, MidpointRounding.AwayFromZero), MinOfEfficiency = g.Min(x => x.Efficiency) }).ToList();
            var efficiencyTableTotalAverage = efficiencyTable.Average(r => r.EfficiencyCal);

            var taskResultObject = new SummaryReportBag();
            taskResultObject.summaryList = summaryList;
            taskResultObject.totalProduction = totalProduction;
            taskResultObject.totalScrapUnits = totalScrapUnits;
            taskResultObject.productionTableList = productionTableList;
            taskResultObject.scrapTableList = scrapTableList;
            taskResultObject.efficiencyTable = efficiencyTable;
            taskResultObject.efficiencyTableTotalAverage = efficiencyTableTotalAverage;


            return Ok(taskResultObject);
        }

        public struct ScrapTableRow
        {
            public string Line { get; set; }
            public int Scrap { get; set; }
            public decimal ScrapPercent { get; set; }

        }
        public struct ProductivitySummaryReportBag
        {
            public IEnumerable<ProductivitySummary> ProductivitySummary { get; set; }
            public IEnumerable<ScrapTableRow> ScrapTable { get; set; }

        }

        // GET: api/ProductivitySummary
        [HttpGet("ProductivitySummary")]
        public async Task<ActionResult<ProductivitySummaryReportBag>> GetProductivitySummary()
        {
            if (_context.ProductivitySummary == null)
            {
                return NotFound();
            }

            var summaryList = await _context.ProductivitySummary.ToListAsync();

            var scrapList = summaryList.GroupBy(x => x.Line).Select(g => new ScrapTableRow() { Line = g.Key, Scrap = g.Sum(x => x.ScrapUnits), ScrapPercent = (g.Sum(x => x.ScrapUnits) / g.Sum(x => x.ScrapDEN)) })
            .OrderBy(o => o.Line)
            .ToList();

            // var productionTableList = baseList.GroupBy(x => x.Line).Select(g => new TableDataRow() { Line = g.Key, Production = g.Sum(x => x.Production), Average = getAverage(totalProduction, g.Sum(x => x.Production)) }).ToList();


            // var scrapTableList = baseList.GroupBy(x => x.Line).Select(g => new ScrapTableDataRow() { Line = g.Key, ScrapUnits = g.Sum(x => x.ScrapUnits), Average = getAverage(totalScrapUnits, g.Sum(x => x.ScrapUnits)) }).ToList();

            // var efficiencyTable = baseList.GroupBy(x => x.Line).Select(g => new EfficiencyTableRow() { Line = g.Key, EfficiencyCal = Math.Round(g.Sum(x => x.Production) / g.Sum(x => x.EffDEN), 2, MidpointRounding.AwayFromZero), MinOfEfficiency = g.Min(x => x.Efficiency) }).ToList();
            // var efficiencyTableTotalAverage = efficiencyTable.Average(r => r.EfficiencyCal);

            var taskResultObject = new ProductivitySummaryReportBag();
            // taskResultObject.message = "OK";
            taskResultObject.ProductivitySummary = summaryList;
            taskResultObject.ScrapTable = scrapList;
            // taskResultObject.totalProduction = totalProduction;
            // taskResultObject.totalScrapUnits = totalScrapUnits;
            // taskResultObject.productionTableList = productionTableList;
            // taskResultObject.scrapTableList = scrapTableList;
            // taskResultObject.efficiencyTable = efficiencyTable;
            // taskResultObject.efficiencyTableTotalAverage = efficiencyTableTotalAverage;


            return Ok(taskResultObject);
        }

        // GET: api/Productivity
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productivity>>> GetProductivity()
        {
            if (_context.Productivity == null)
            {
                return NotFound();
            }

            var productivityList = await _context.Productivity
            .Include(x => x.Supervisor)
            .Include(e => e.Line)
            .Include(e => e.Shift)
            .Include(e => e.Hour)
            .Include(e => e.ProductivityFlows).ThenInclude(pf => pf.Product)
            .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeCategory)
            .OrderByDescending(p => p.Id)
            .Take(100)
            .ToListAsync();

            return productivityList;
        }

        public struct ValidationResult
        {
            public Boolean ValidationPassed { get; set; }

        }
        // GET: api/AdditionalValidations
        [HttpGet("AdditionalValidations")]
        public async Task<ActionResult<IEnumerable<Productivity>>> GetAdditionalValidations(
            string date, string line, string shift, string hour)
        {
            if (_context.Productivity == null)
            {
                return NotFound();
            }

            var productivityList = await _context.Productivity
            .Where(p => p.Date == Convert.ToDateTime(date))
            .Where(p => p.LineID == int.Parse(line))
            .Where(p => p.ShiftID == int.Parse(shift))
            .Where(p => p.HourID == int.Parse(hour))
            .ToListAsync();

            // return productivityList;
            var result = false;
            if (productivityList.Count() == 0) result = true;

            return new OkObjectResult(new ValidationResult { ValidationPassed = result });
        }

        public class ProductivityQuery
        {
            public string? DateFrom { get; set; } = null;
            public string? DateTo { get; set; } = null;
            public int? LineId { get; set; } = null;
            public int? SupervisorId { get; set; } = null;
            public string? Sku { get; set; } = null;
            public string? startHour { get; set; } = null;
            public string? endHour { get; set; } = null;
        }

        // GET: api/ProductivityFiltered
        [HttpGet("ProductivityFiltered")]
        public ActionResult<IEnumerable<Productivity>> GetProductivityFiltered(
            [FromQuery] ProductivityQuery productivityQuery)
        {
            if (_context.Productivity == null)
            {
                return NotFound();
            }

            IQueryable<Productivity> query = _context.Productivity;

            query = query.Include(p => p.Line)
            .Include(p => p.Shift)
            .Include(p => p.Supervisor);

            if (productivityQuery.LineId != null)
            {
                query = query.Where(p => p.LineID == productivityQuery.LineId);
            }
            if (productivityQuery.SupervisorId != null)
            {
                query = query.Where(p => p.SupervisorID == productivityQuery.SupervisorId);
            }
            if (productivityQuery.Sku != null)
            {
                query = query.Where(p => p.Sku == productivityQuery.Sku);
            }
            if (productivityQuery.DateFrom != null)
            {
                query = query.Where(p => p.Date >= Convert.ToDateTime(productivityQuery.DateFrom));
            }
            if (productivityQuery.DateTo != null)
            {
                query = query.Where(p => p.Date <= Convert.ToDateTime(productivityQuery.DateTo));
            }
            if (productivityQuery.startHour != null)
            {
                query = query.Where(p => p.HourStart == productivityQuery.startHour);
            }
            if (productivityQuery.endHour != null)
            {
                query = query.Where(p => p.HourEnd == productivityQuery.endHour);
            }

            List<Productivity> productivityList = query.Take(5000).OrderBy(p => p.Date).ToList();

            return productivityList;
        }

        // GET: api/Productivity/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Productivity>> GetProductivity(int id)
        {
            if (_context.Productivity == null)
            {
                return NotFound();
            }
            var productivity = await _context.Productivity.Include(e => e.ProductivityFlows).ThenInclude(pf => pf.Product)
            .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeCategory)
            .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeSubCategory1)
            .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeSubCategory2)
            .Include(e => e.DowntimeReasons).ThenInclude(pr => pr.DowntimeCode)
            .FirstOrDefaultAsync(i => i.Id == id);

            if (productivity == null)
            {
                return NotFound();
            }

            return productivity;
        }

        // PUT: api/Productivity/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductivity(int id, Productivity productivity)
        {
            // Edit productivity
            if (id != productivity.Id)
            {
                return BadRequest();
            }

            int[] currentInts = _context.DowntimeReason.Where(x => x.ProductivityID == productivity.Id).Select(x => x.Id).ToArray();

            int[] payloadInts = productivity.DowntimeReasons.Where(x => x.ProductivityID == productivity.Id).Select(x => x.Id).ToArray();
            for (int i = 0; i < currentInts.Length; i++)
            {
                id = currentInts[i];
                if (!payloadInts.Contains(id))
                {
                    // DELETE
                    _context.DowntimeReason.Where(x => x.Id == id).ExecuteDelete();
                }
            }

            foreach (var reason in productivity.DowntimeReasons.ToArray())
            {
                if (reason.Id == 0)
                {
                    _context.Entry(reason).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(reason).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();

            foreach (var flow in productivity.ProductivityFlows.ToArray())
            {
                _context.Entry(flow).State = EntityState.Modified;
            }

            _context.Entry(productivity).State = EntityState.Modified;


            try
            {
                // Normalize Some Productivity and Flow Fields            
                for (int i = 0; i < productivity.ProductivityFlows.Count(); i++)
                {
                    var flow = productivity.ProductivityFlows.ToList()[i];

                    if (flow.ProductID == 0)
                    {
                        flow.ProductID = null;
                    }
                    if (flow.ProductID > 0)
                    {
                        var product = await _context.Product.FindAsync(flow.ProductID);
                        if (product != null)
                        {
                            if (i == 0)
                            {
                                productivity.Sku = product.Sku;
                            }
                            if (i == 1)
                            {
                                productivity.Sku2 = product.Sku;
                            }
                        }

                    }

                }

                var supervisor = await _context.Supervisor.FindAsync(productivity.SupervisorID);

                if (supervisor != null)
                {
                    productivity.Name = supervisor.Name;
                }

                var hour = await _context.Hour.FindAsync(productivity.HourID);

                if (hour != null)
                {
                    DateTimeFormatInfo dateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
                    TimeSpan oneHour = new TimeSpan(1, 0, 0);
                    DateTime dateTime;

                    if (DateTime.TryParse(hour.Time, dateTimeFormatInfo, out dateTime))
                    {
                        dateTime = dateTime.Add(oneHour);

                        // Print the resulting DateTime object.
                        productivity.HourStart = $"{hour.Time}";
                        productivity.HourEnd = $"{dateTime.ToString("hh:mm tt")}";
                    }

                }


                // Define Working Product and Calculate Some Totals
                Product? workingProduct = await GetWorkingProduct(productivity.ProductivityFlows);

                var totalProduction = 0;
                totalProduction = productivity.ProductivityFlows.Aggregate(0, (acc, x) => acc + Decimal.ToInt32(x.Production));

                if (workingProduct != null)
                {
                    var standardSpeed = workingProduct.StandardSpeed;
                    var remainingProduction = standardSpeed - totalProduction;
                    decimal remainingMinutes = (decimal)remainingProduction / standardSpeed * 60;

                    productivity.RemainingProduction = remainingProduction;
                    productivity.RemainingMinutes = remainingMinutes;
                    productivity.StandardSpeed = standardSpeed;
                }

                // Calculate Total Downtime
                Decimal totalDowntimeMinutes = 0;
                totalDowntimeMinutes = productivity.DowntimeReasons.Aggregate(0, (Decimal acc, DowntimeReason x) => acc + x.Minutes);

                productivity.DowntimeMinutes = totalDowntimeMinutes;

                await _context.SaveChangesAsync();


            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductivityExists(id))
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



        private async Task<Product?> GetWorkingProduct(ICollection<ProductivityFlow> flows)
        {
            int workingProductId = 0;
            var draftProduct = flows
            .Select(flow => _context.Product.FirstOrDefault(product => product.Id == flow.ProductID))
            .ToList();

            Product? workingProduct = null;

            if (draftProduct.Count == 1)
            {
                // Line with one flow

                workingProductId = draftProduct[0]?.Id ?? 0;
            }
            else if (draftProduct.Count == 2)
            {
                // Line with two flows

                int standardSpeedOne = draftProduct[0]?.StandardSpeed ?? 0;
                int standardSpeedTwo = draftProduct[1]?.StandardSpeed ?? 0;

                if (standardSpeedOne > standardSpeedTwo)
                {
                    workingProductId = draftProduct[0]?.Id ?? 0;
                }
                else
                {
                    workingProductId = draftProduct[1]?.Id ?? 0;
                }
            }

            workingProduct = await _context.Product.FindAsync(workingProductId);
            return workingProduct;
        }

        // POST: api/Productivity
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Productivity>> PostProductivity(Productivity productivity)
        {
            if (_context.Productivity == null)
            {
                return Problem("Entity set 'ApplicationDBContext.Productivity'  is null.");
            }

            // Normalize Some Productivity and Flow Fields
            int flowCount = productivity.ProductivityFlows.Count();
            productivity.Flow = flowCount;

            for (int i = 0; i < flowCount; i++)
            {
                var flow = productivity.ProductivityFlows.ToList()[i];

                if (flow.ProductID == 0)
                {
                    flow.ProductID = null;
                }
                if (flow.ProductID > 0)
                {
                    var product = await _context.Product.FindAsync(flow.ProductID);
                    if (product != null)
                    {
                        if (i == 0)
                        {
                            productivity.Sku = product.Sku;
                        }
                        if (i == 1)
                        {
                            productivity.Sku2 = product.Sku;
                        }
                    }

                }

            }

            var supervisor = await _context.Supervisor.FindAsync(productivity.SupervisorID);
            if (supervisor != null)
            {
                productivity.Name = supervisor.Name;
            }

            var hour = await _context.Hour.FindAsync(productivity.HourID);
            if (hour != null)
            {
                DateTimeFormatInfo dateTimeFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
                TimeSpan oneHour = new TimeSpan(1, 0, 0);
                DateTime dateTime;

                if (DateTime.TryParse(hour.Time, dateTimeFormatInfo, out dateTime))
                {
                    dateTime = dateTime.Add(oneHour);

                    // Print the resulting DateTime object.
                    productivity.HourStart = $"{hour.Time}";
                    productivity.HourEnd = $"{dateTime.ToString("hh:mm tt")}";
                }

            }


            // Define Working Product and Calculate Some Totals
            Product? workingProduct = await GetWorkingProduct(productivity.ProductivityFlows);

            var totalProduction = 0;
            totalProduction = productivity.ProductivityFlows.Aggregate(0, (acc, x) => acc + Decimal.ToInt32(x.Production));

            if (workingProduct != null)
            {
                var standardSpeed = workingProduct.StandardSpeed;
                var remainingProduction = standardSpeed - totalProduction;
                decimal remainingMinutes = (decimal)remainingProduction / standardSpeed * 60;

                productivity.RemainingProduction = remainingProduction;
                productivity.RemainingMinutes = remainingMinutes;
                productivity.StandardSpeed = standardSpeed;
            }

            // Calculate Total Downtime
            Decimal totalDowntimeMinutes = 0;
            totalDowntimeMinutes = productivity.DowntimeReasons.Aggregate(0, (Decimal acc, DowntimeReason x) => acc + x.Minutes);

            productivity.DowntimeMinutes = totalDowntimeMinutes;

            // Save        
            _context.Productivity.Add(productivity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductivity", new { id = productivity.Id }, productivity);
        }

        // DELETE: api/Productivity/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductivity(int id)
        {
            if (_context.Productivity == null)
            {
                return NotFound();
            }
            var productivity = await _context.Productivity.FindAsync(id);
            if (productivity == null)
            {
                return NotFound();
            }

            _context.ProductivityFlow.RemoveRange(_context.ProductivityFlow.Where(x => x.ProductivityID == id));
            _context.SaveChanges();

            _context.Productivity.Remove(productivity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductivityExists(int id)
        {
            return (_context.Productivity?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost("distinct-lines-by-date")]
        public ActionResult<IEnumerable<DTOLineInfo>> GetDistinctLinesByDate([FromBody] DTODateRequest request)
        {
            var distinctLineInfo = _context.Productivity
                .Where(p => p.Date == request.TargetDate)
                .Select(p => new DTOLineInfo { Id = p.Line != null ? p.Line.Id : 0, Name = p.Line != null ? p.Line.Name : "Unknown" })
                .Distinct()
                .ToList();

            return distinctLineInfo;
        }

        [HttpPost("distinct-shifts-by-date")]
        public ActionResult<IEnumerable<DTOShiftInfo>> GetDistinctShiftsByDate([FromBody] DTODistinctShiftRequest request)
        {
            var distinctShifts = _context.Productivity
                .Where(p => p.Date == request.TargetDate && p.LineID == request.LineId)
                .Select(p => new DTOShiftInfo { Id = p.Shift != null ? p.Shift.Id : 0, Name = p.Shift != null ? p.Shift.Name : "Unknown" })
                .Distinct()
                .ToList();

            return distinctShifts;
        }

        /*         [HttpGet("generate-excel-report")] // Deprecated
                public IActionResult ExportExcelProductivity([FromQuery] ProductivityQuery productivityQuery)
                {
                    if (_context.ProductivityReport == null)
                    {
                        return NotFound();
                    }

                    IQueryable<ProductivityReport> query = _context.ProductivityReport;

                    if (productivityQuery.LineId != null)
                    {
                        query = query.Where(p => p.LineID == productivityQuery.LineId);
                    }
                    if (productivityQuery.SupervisorId != null)
                    {
                        query = query.Where(p => p.SupervisorID == productivityQuery.SupervisorId);
                    }
                    if (productivityQuery.Sku != null)
                    {
                        query = query.Where(p => p.Sku == productivityQuery.Sku);
                    }
                    if (productivityQuery.DateFrom != null)
                    {
                        query = query.Where(p => p.Date >= Convert.ToDateTime(productivityQuery.DateFrom));
                    }
                    if (productivityQuery.DateTo != null)
                    {
                        query = query.Where(p => p.Date <= Convert.ToDateTime(productivityQuery.DateTo));
                    }
                    if (productivityQuery.startHour != null)
                    {
                        query = query.Where(p => p.HourStart == productivityQuery.startHour);
                    }
                    if (productivityQuery.endHour != null)
                    {
                        query = query.Where(p => p.HourEnd == productivityQuery.endHour);
                    }

                    List<ProductivityReport> productivityReports = query.Take(5000).OrderBy(p => p.Date).ToList();

                    return GenerateExcelProductivity(productivityReports);
                }

                public FileResult GenerateExcelProductivity(List<ProductivityReport> data)
                {
                    DataTable sheetProduction = new DataTable("Production");
                    DataTable sheetDowntime = new DataTable("Downtime");
                    sheetProduction.Columns.AddRange(new DataColumn[]
                    {
                        new DataColumn("Date"),
                        new DataColumn("Line"),
                        new DataColumn("SKU"),
                        new DataColumn("Flavor"),
                        new DataColumn("Shift"),
                        new DataColumn("Production", typeof(int)),
                        new DataColumn("Standard Speed", typeof(int)),
                        new DataColumn("Hour"),
                        new DataColumn("Supervisor"),
                        new DataColumn("Minutes", typeof(double)),
                        new DataColumn("NetContent"),
                        new DataColumn("Packing"),
                        new DataColumn("Flow", typeof(int)),
                    });
                    sheetDowntime.Columns.AddRange(new DataColumn[]{
                        new DataColumn("Date"),
                        new DataColumn("Hour"),
                        new DataColumn("Shift"),
                        new DataColumn("Line"),
                        new DataColumn("SKU"),
                        new DataColumn("Supervisor"),
                        new DataColumn("Code"),
                        new DataColumn("Categories"),
                        new DataColumn("Sub Categories 1"),
                        new DataColumn("Sub Categories 2"),
                        new DataColumn("Minutes", typeof(double)),
                        new DataColumn("Flow", typeof(int))
                    });

                    int? idTemp = 0;
                    foreach (var row in data)
                    {
                        if (idTemp != row.Id)
                        {
                            sheetProduction.Rows.Add(
                                row.Date.ToString("MM/dd/yyyy"),
                                row.Line,
                                row.Sku,
                                row.Flavor,
                                row.Shift,
                                row.Production,
                                row.StandardSpeed,
                                row.HourStart + " - " + row.HourEnd,
                                row.Supervisor,
                                row.Downtime_minutes,
                                row.NetContent,
                                row.Packing,
                                1
                            );
                            if (row.Flow == 2)
                            {
                                sheetProduction.Rows.Add(
                                row.Date.ToString("MM/dd/yyyy"),
                                row.Line,
                                row.Sku2,
                                row.Flavor2,
                                row.Shift,
                                row.Production2,
                                row.StandardSpeed,
                                row.HourStart + " - " + row.HourEnd,
                                row.Supervisor,
                                row.Downtime_minutes,
                                row.NetContent2,
                                row.Packing2,
                                2
                            );
                            }
                            if (row.Downtime_minutes != 0)
                            {
                                sheetDowntime.Rows.Add(
                                    row.Date.ToString("MM/dd/yyyy"),
                                    row.HourStart + " - " + row.HourEnd,
                                    row.Shift,
                                    row.Line,
                                    row.FlowIndex == 1 ? row.Sku : row.Sku2,
                                    row.Supervisor,
                                    row.Code,
                                    row.Category,
                                    row.SubCategory1,
                                    row.SubCategory2,
                                    row.Minutes,
                                    row.FlowIndex
                                );
                            }
                        }
                        else
                        {
                            sheetDowntime.Rows.Add(
                                row.Date.ToString("MM/dd/yyyy"),
                                row.HourStart + " - " + row.HourEnd,
                                row.Shift,
                                row.Line,
                                row.FlowIndex == 1 ? row.Sku : row.Sku2,
                                row.Supervisor,
                                row.Code,
                                row.Category,
                                row.SubCategory1,
                                row.SubCategory2,
                                row.Minutes,
                                row.FlowIndex
                            );
                        }
                        idTemp = row.Id;
                    };

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(sheetProduction);
                        wb.Worksheets.Add(sheetDowntime);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "reportProductivity.xlsx");
                        }
                    }
                } */


        //* ---------------------- Refactor Productivity records --------------------- */
        //* --------------------------------- Filters -------------------------------- */
        [HttpPost("lines")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetLines(ProductivityFilter filters)
        {
            try
            {
                var lines = await _productionRepository.GetLines(filters);
                return Ok(lines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("supervisor")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetSupervisor(ProductivityFilter filters)
        {
            try
            {
                var supervisors = await _productionRepository.GetSupervisor(filters);
                return Ok(supervisors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("shift")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetShift(ProductivityFilter filters)
        {
            try
            {
                var shifts = await _productionRepository.GetShift(filters);
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("sku")]
        public async Task<ActionResult<IEnumerable<DTOReactDropdown<int>>>> GetSku(ProductivityFilter filters)
        {
            try
            {
                var skus = await _productionRepository.GetSku(filters);
                return Ok(skus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //* ---------------------------------- Table --------------------------------- */
        [HttpPost("records")]
        public async Task<ActionResult<DTOResultRecords>> GetProductivityRecords(ProductivityFilter filters)
        {
            try
            {
                var records = await _productionRepository.GetProductivityRecords(filters);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //* ----------------------------- Generated Excel ---------------------------- */
        [HttpPost("excel-records")]
        public IActionResult ExportExcelRecrods(ProductivityFilter filters)
        {
            try
            {
                DateTime startDate = DateTime.Parse(filters.StartDate);
                DateTime endDate = DateTime.Parse(filters.EndDate);

                IQueryable<ProductivityReport> query = _context.ProductivityReport
                .Where(w => w.Date >= startDate && w.Date <= endDate);



                if (filters.Lines.Any())
                {
                    query = query.Where(w => filters.Lines.Contains(w.LineID.GetValueOrDefault(0)));
                }

                if (filters.Supervisor.Any())
                {
                    query = query.Where(w => filters.Supervisor.Contains(w.SupervisorID.GetValueOrDefault(0)));
                }

                if (filters.Shift.Any())
                {
                    query = query.Where(w => filters.Shift.Contains(w.ShiftID.GetValueOrDefault(0)));
                }

                if (filters.Sku.Any())
                {
                    query = query.Where(w => filters.Sku.Contains(w.ProductID.GetValueOrDefault(0)) || filters.Sku.Contains(w.ProductID2.GetValueOrDefault(0)));
                }

                int count = query.Count();

                List<ProductivityReport> productivityData = query
                .OrderBy(o => o.Date)
                .Take(5000)
                .ToList();

                // save all distinct IDs in productivityData to a list
                int[] distinctIds = productivityData
                .Select(p => p.Id.GetValueOrDefault(0))
                .Distinct()
                .ToArray();

                var downtimeData = _context.DowntimeReason
                    .Where(dr => distinctIds.Contains(dr.ProductivityID))
                    .Select(dr => new DTODowntimeReportExcel
                    {
                        Date = dr.Productivity != null ? dr.Productivity.Date : default,
                        HourStart = dr.Productivity != null ? dr.Productivity.HourStart : string.Empty,
                        HourEnd = dr.Productivity != null ? dr.Productivity.HourEnd : string.Empty,
                        Shift = dr.Productivity != null && dr.Productivity.Shift != null ? dr.Productivity.Shift.Name : string.Empty,
                        Line = dr.Productivity != null && dr.Productivity.Line != null ? dr.Productivity.Line.Name : string.Empty,
                        Sku = dr.Productivity != null ? dr.Productivity.Sku : string.Empty,
                        Sku2 = dr.Productivity != null ? dr.Productivity.Sku2 : string.Empty,
                        Supervisor = dr.Productivity != null ? dr.Productivity.Name : string.Empty,
                        Code = dr.DowntimeCode != null ? dr.DowntimeCode.Code : string.Empty,
                        Category = dr.DowntimeCategory != null ? dr.DowntimeCategory.Name : string.Empty,
                        Subcategory1 = dr.DowntimeSubCategory1 != null ? dr.DowntimeSubCategory1.Name : string.Empty,
                        Subcategory2 = dr.DowntimeSubCategory2 != null ? dr.DowntimeSubCategory2.Name : string.Empty,
                        Minutes = dr.Minutes,
                        Flow = dr.FlowIndex
                    })
                    .ToList();

                // Divide productivityData into two lists based on Flow
                // productivityData count 235
                var flow1Data = productivityData.Where(p => p.Flow == 1).ToList(); // Flow 1 count 0
                var flow2Data = productivityData.Where(p => p.Flow == 2).ToList(); // flow 2 count 235


                //* Excel */
                DataTable sheetProduction = new DataTable("Production");
                DataTable sheetDowntime = new DataTable("Downtime");
                sheetProduction.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Date"),
                    new DataColumn("Line"),
                    new DataColumn("SKU"),
                    new DataColumn("Flavor"),
                    new DataColumn("Shift"),
                    new DataColumn("Production", typeof(int)),
                    new DataColumn("Standard Speed", typeof(int)),
                    new DataColumn("Hour"),
                    new DataColumn("Supervisor"),
                    new DataColumn("Minutes", typeof(double)),
                    new DataColumn("NetContent"),
                    new DataColumn("Packing"),
                    new DataColumn("Flow", typeof(int)),
                });
                sheetDowntime.Columns.AddRange(new DataColumn[]{
                    new DataColumn("Date"),
                    new DataColumn("Hour"),
                    new DataColumn("Shift"),
                    new DataColumn("Line"),
                    new DataColumn("SKU"),
                    new DataColumn("Supervisor"),
                    new DataColumn("Code"),
                    new DataColumn("Category"),
                    new DataColumn("Subcategory 1"),
                    new DataColumn("Subcategory 2"),
                    new DataColumn("Minutes", typeof(double)),
                    new DataColumn("Flow", typeof(int))
                });

                foreach (var row in productivityData)
                {

                    if (row.Flow == 1)
                    {
                        sheetProduction.Rows.Add(
                            row.Date.ToString("MM/dd/yyyy"),
                            row.Line,
                            row.Sku,
                            row.Flavor,
                            row.Shift,
                            row.Production,
                            row.StandardSpeed,
                            row.HourStart + " - " + row.HourEnd,
                            row.Supervisor,
                            row.Downtime_minutes,
                            row.NetContent,
                            row.Packing,
                            1
                        );
                    }

                    if (row.Flow == 2)
                    {
                        sheetProduction.Rows.Add(
                            row.Date.ToString("MM/dd/yyyy"),
                            row.Line,
                            row.Sku,
                            row.Flavor,
                            row.Shift,
                            row.Production,
                            row.StandardSpeed,
                            row.HourStart + " - " + row.HourEnd,
                            row.Supervisor,
                            row.Downtime_minutes,
                            row.NetContent,
                            row.Packing,
                            1
                        );

                        sheetProduction.Rows.Add(
                            row.Date.ToString("MM/dd/yyyy"),
                            row.Line,
                            row.Sku2,
                            row.Flavor2,
                            row.Shift,
                            row.Production2,
                            row.StandardSpeed,
                            row.HourStart + " - " + row.HourEnd,
                            row.Supervisor,
                            row.Downtime_minutes,
                            row.NetContent2,
                            row.Packing2,
                            2
                        );
                    }

                }

                foreach (var row in downtimeData)
                {

                    sheetDowntime.Rows.Add(
                    row.Date.ToString("MM/dd/yyyy"),
                    row.HourStart + " - " + row.HourEnd,
                    row.Shift,
                    row.Line,
                    row.Flow == 1 ? row.Sku : row.Sku2,
                    row.Supervisor,
                    row.Code,
                    row.Category,
                    row.Subcategory1,
                    row.Subcategory2,
                    row.Minutes,
                    row.Flow
                );

                }

                using XLWorkbook wb = new XLWorkbook();
                wb.Worksheets.Add(sheetProduction);
                wb.Worksheets.Add(sheetDowntime);
                using MemoryStream stream = new MemoryStream();
                wb.SaveAs(stream);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "reportProductivity.xlsx");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

}