using Microsoft.AspNetCore.Mvc;
using Inventio.Data;
using Inventio.Models.Views;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using inventio.Models.DTO.ProductionSummary;
using Irony.Parsing;
using System.Data;
using ClosedXML.Excel;
using inventio.Repositories.EntityFramework;
using Microsoft.Data.SqlClient;
using inventio.Models.DTO.Supervisor;

namespace inventio.Controllers
{
    [Route("api/production-summary")]
    [ApiController]
    public class ProductionSummaryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IEFRepository<DTOGetEfficiency> _PCLrepository;

        public ProductionSummaryController(ApplicationDBContext context, IEFRepository<DTOGetEfficiency> PCLrepository)
        {
            _context = context;
            _PCLrepository = PCLrepository;
        }

        public static int GetQuarter(DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        // API HTTP METHODS

        //* CASES ANALYSIS

        [HttpPost("total-cases")]
        public async Task<ActionResult<TotalCases>> GetTotalCases(CasesAnalysisFilter filters)
        {

            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }



            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductionSummary
              .Where(ps => ps.Date >= startDate && ps.Date <= endDate)
            .SumAsync(ps => ps.Production);


            TotalCases resultFormatted = new()
            {
                TotalProduction = Convert.ToInt32(result)
            };


            return Ok(resultFormatted);
        }

        [HttpPost("total-cases-Line")]
        public async Task<ActionResult<TotalCasesByLineFormatted>> GetTotalCasesByLine(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }



            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .Where(l => filters.Lines.Contains(l.Line)).ToListAsync();

            var result = data.GroupBy(x => x.Line)
            .Select(g => new { Line = g.Key, Production = g.Sum(x => x.Production) }).ToList();



            if (result.Count == 0)
            {
                TotalCasesByLineFormatted abort = new();
                return abort;
            }

            int sumTotal = 0;
            List<TotalCasesByLine> tempList = new();

            for (int i = 0; i < result.Count; i++)
            {

                decimal production = result[i].Production ?? 0;
                sumTotal += Convert.ToInt32(production);
            }

            for (int i = 0; i < result.Count; i++)
            {

                decimal production = result[i].Production ?? 0;

                TotalCasesByLine aux = new()
                {
                    Line = result[i].Line,
                    Production = Convert.ToInt32(production),
                    Percent = Math.Round((production * 100) / sumTotal, 1)
                };
                tempList.Add(aux);
            }




            TotalCasesByLineFormatted resultFormatted = new()
            {
                Data = tempList,
                SumTotal = sumTotal
            };

            return Ok(resultFormatted);
        }

        [HttpPost("total-cases-shift")]
        public async Task<ActionResult<IEnumerable<ProductionSummary>>> GetTotalCasesByShift(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }


            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductionSummary
            .Where(obj => filters.Lines.Contains(obj.Line) && obj.Date >= startDate && obj.Date <= endDate)
            .GroupBy(x => x.Shift)
            .Select(g => new { type = g.Key, value = g.Sum(x => x.Production) }).ToListAsync();


            int sumTotal = 0;
            List<TotalCasesByShift> tempList = new();

            for (int i = 0; i < result.Count; i++)
            {

                decimal production = result[i].value ?? 0;
                TotalCasesByShift aux = new()
                {
                    Type = result[i].type,
                    Value = Convert.ToInt32(production)
                };
                tempList.Add(aux);
                sumTotal += Convert.ToInt32(production);
            }

            TotalCasesByShiftFormatted resultFormatted = new()
            {
                Data = tempList,
                SumTotal = sumTotal
            };

            return Ok(resultFormatted);
        }



        [HttpPost("total-cases-by-day")]
        public async Task<ActionResult<IEnumerable<TotalCasesByDate>>> GetTotalCasesByDay(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }


            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .Where(l => filters.Lines.Contains(l.Line)).ToListAsync();

            var result = data.GroupBy(x => new
            {
                x.Date,
                x.Line
            })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.Line,
                Production = g.Sum(x => x.Production)
            })
            .OrderBy(y => y.Date).ToList();

            List<TotalCasesByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal production = result[i].Production ?? 0;

                TotalCasesByDate aux = new()
                {
                    Time = result[i].Date.ToString("MM/dd/yy"),
                    Type = result[i].Line,
                    Value = Convert.ToInt32(production)
                };

                resultFormatted.Add(aux);
            }

            return Ok(resultFormatted);

        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        [HttpPost("total-cases-by-week")]
        public async Task<ActionResult<IEnumerable<TotalCasesByDate>>> GetTotalCasesByWeek(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var baseList = await _context.ProductionSummary.Where(x => filters.Lines.Contains(x.Line) && x.Date >= startDate && x.Date <= endDate).ToListAsync();
            var result = baseList.GroupBy(x => new { line = x.Line, week = GetIso8601WeekOfYear(x.Date), year = x.Date.Year })
            .Select(g => new { week = g.Key.week, year = g.Key.year, line = g.Key.line, production = g.Sum(x => x.Production) }).OrderBy(o => o.year).ThenBy(o => o.week).ToList();


            List<TotalCasesByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal production = result[i].production ?? 0;

                TotalCasesByDate aux = new()
                {
                    Time = result[i].week.ToString() + "/" + result[i].year.ToString(),
                    Type = result[i].line,
                    Value = Convert.ToInt32(production)
                };

                resultFormatted.Add(aux);
            }

            return Ok(resultFormatted);

        }




        [HttpPost("total-cases-by-month")]
        public async Task<ActionResult<IEnumerable<TotalCasesByDate>>> GetTotalCasesByMonth(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }


            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .Where(l => filters.Lines.Contains(l.Line)).ToListAsync();

            var result = data.GroupBy(x => new
            {
                x.Date.Month,
                x.Date.Year,
                x.Line
            })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.Year,
                g.Key.Line,
                Production = g.Sum(x => x.Production)
            })
            .OrderBy(y => y.Year)
            .ThenBy(m => m.Month).ToList();

            List<TotalCasesByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal production = result[i].Production ?? 0;

                TotalCasesByDate aux = new()
                {
                    Time = result[i].Year.ToString() + "/" + result[i].Month.ToString(),
                    Type = result[i].Line,
                    Value = Convert.ToInt32(production)
                };

                resultFormatted.Add(aux);
            }

            return Ok(resultFormatted);
        }


        [HttpPost("total-cases-by-quarter")]
        public async Task<ActionResult<IEnumerable<TotalCasesByDate>>> GetTotalCasesByQuarter(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }



            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var baseList = await _context.ProductionSummary.Where(x => filters.Lines.Contains(x.Line) && x.Date >= startDate && x.Date <= endDate).ToListAsync();
            var result = baseList.GroupBy(x => new { line = x.Line, quarter = GetQuarter(x.Date), year = x.Date.Year })
            .Select(g => new { quarter = g.Key.quarter, year = g.Key.year, line = g.Key.line, production = g.Sum(x => x.Production) })
            .OrderBy(o => o.year)
            .ThenBy(o => o.quarter)
            .ToList();


            List<TotalCasesByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal production = result[i].production ?? 0;

                TotalCasesByDate aux = new()
                {
                    Time = result[i].quarter.ToString() + "/" + result[i].year.ToString(),
                    Type = result[i].line,
                    Value = Convert.ToInt32(production)
                };

                resultFormatted.Add(aux);
            }

            return Ok(resultFormatted);
        }


        [HttpPost("total-cases-by-year")]
        public async Task<ActionResult<IEnumerable<TotalCasesByDate>>> GetTotalCasesByYear(CasesAnalysisFilter filters)
        {
            if (_context.ProductionSummary == null)
            {
                return NotFound();
            }


            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .Where(l => filters.Lines.Contains(l.Line)).ToListAsync();

            var result = data.GroupBy(x => new
            {
                x.Date.Year,
                x.Line
            })
            .Select(g => new
            {
                time = g.Key.Year,
                type = g.Key.Line,
                value = g.Sum(x => x.Production)
            })
            .OrderBy(y => y.time).ToList();



            return Ok(result);

        }



        //* --------------------------- Efficiency Analysis -------------------------- */
        [HttpPost("efficiency-filter")]
        public ActionResult<IEnumerable<string>> GetLine(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);
            var data = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            )
            .Select(s => s.Line)
            .Distinct()
            .ToList();

            return data;
        }


        [HttpPost("efficiency")]
        public ActionResult<Efficiency> GetEfficiency(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);
            var data = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            )
            .Where(w => filters.Lines!.Contains(w.Line))
            .ToList();

            decimal? Production = data.Sum(s => s.Production);
            decimal? MaxProduction = data.Sum(s => s.MaxProduction);


            Efficiency resultFormatted = new()
            {
                TotalEfficiency = (decimal)((Production ?? 0) / (MaxProduction == 0 ? 1 : MaxProduction))!

            };

            return resultFormatted;
        }


        [HttpPost("efficiency-line")]
        public ActionResult<EfficiencyByLineFormatted> GetEfficiencyByLine(EfficiencyFilter filters)
        {

            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            )
            .Where(w => filters.Lines!.Contains(w.Line))
            .GroupBy(g => g.Line)
            .Select(g => new
            {
                Line = g.Key,
                Production = g.Sum(ps => ps.Production),
                MaxProduction = g.Sum(ps => ps.MaxProduction)
            })
            .ToList();


            decimal? Production = data.Sum(s => s.Production);
            decimal? MaxProduction = data.Sum(s => s.MaxProduction);

            decimal? totalEff = (Production ?? 0) / (MaxProduction == 0 ? 1 : MaxProduction);

            if (data.Count == 0)
            {
                EfficiencyByLineFormatted abort = new();
                return abort;
            }

            List<EfficiencyByLine> tempList = new();

            foreach (var item in data)
            {
                tempList.Add(new EfficiencyByLine
                {
                    Line = item.Line,
                    Efficiency = (item.Production ?? 0) / (item.MaxProduction ?? 1)
                });
            }

            EfficiencyByLineFormatted resultFormatted = new()
            {
                Table = tempList,
                TotalEfficiency = totalEff.GetValueOrDefault(0)
            };

            return resultFormatted;
        }

        [HttpPost("efficiency-shift")]
        public ActionResult<IEnumerable<ProductionSummary>> GetEfficiencyByShift(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            )
            .Where(ps => ps.Date >= startDate && ps.Date <= endDate && filters.Lines!.Contains(ps.Line))
            .GroupBy(ps => ps.Shift)
            .Select(group => new
            {
                Shift = group.Key,
                Efficiency = group.Sum(ps => ps.Production) / group.Sum(ps => ps.MaxProduction),
            })
            .ToList();

            return Ok(data);
        }



        [HttpPost("efficiency-by-day")]
        public ActionResult<IEnumerable<EfficiencyByDate>> GetEfficiencyByDay(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            ).Where(ps => filters.Lines!.Contains(ps.Line))
             .GroupBy(x => new
             {
                 x.Date,
                 x.Line
             })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.Line,
                Efficiency = g.Sum(ps => ps.Production) / g.Sum(ps => ps.MaxProduction),
            })
            .OrderBy(y => y.Date)
            .ToList();

            List<EfficiencyByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal efficiency = result[i].Efficiency ?? 0;

                EfficiencyByDate aux = new()
                {
                    Time = result[i].Date.ToString("MM/dd/yy"),
                    Type = result[i].Line,
                    Value = Math.Round((efficiency * 100), 1)
                };

                resultFormatted.Add(aux);
            }

            return Ok(resultFormatted);

        }

        [HttpPost("efficiency-by-week")]
        public ActionResult<IEnumerable<EfficiencyByDate>> GetEfficiencyByWeek(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var baseList = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            ).Where(x => filters.Lines!.Contains(x.Line))
            .ToList();


            //var baseList = await _context.ProductionSummary.Where(x => filters.Lines!.Contains(x.Line) && x.Date >= startDate && x.Date <= endDate).ToListAsync();
            var result = baseList.GroupBy(x => new { line = x.Line, week = GetIso8601WeekOfYear(x.Date), year = x.Date.Year })
            .Select(g => new { week = g.Key.week, year = g.Key.year, line = g.Key.line, efficiency = g.Sum(ps => ps.Production) / g.Sum(ps => ps.MaxProduction) })
            .OrderBy(o => o.year).ThenBy(o => o.week).ToList();


            List<EfficiencyByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal efficiency = result[i].efficiency ?? 0;

                EfficiencyByDate aux = new()
                {
                    Time = result[i].week.ToString() + "/" + result[i].year.ToString(),
                    Type = result[i].line,
                    Value = Math.Round((efficiency * 100), 1)
                };

                resultFormatted.Add(aux);
            }

            return resultFormatted;

        }


        [HttpPost("efficiency-by-month")]
        public ActionResult<IEnumerable<EfficiencyByDate>> GetEfficiencyByMonth(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            ).Where(ps => ps.Date >= startDate && ps.Date <= endDate && filters.Lines!.Contains(ps.Line))
            .GroupBy(x => new
            {
                x.Date.Month,
                x.Date.Year,
                x.Line
            })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.Year,
                g.Key.Line,
                Efficiency = g.Sum(ps => ps.Production) / g.Sum(ps => ps.MaxProduction),
            })
            .OrderBy(y => y.Year)
            .ThenBy(m => m.Month).ToList();

            List<EfficiencyByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal efficiency = result[i].Efficiency ?? 0;

                EfficiencyByDate aux = new()
                {
                    Time = result[i].Year.ToString() + "/" + result[i].Month.ToString(),
                    Type = result[i].Line,
                    Value = Math.Round((efficiency * 100), 1)
                };

                resultFormatted.Add(aux);
            }

            return resultFormatted;
        }

        [HttpPost("efficiency-by-quarter")]
        public ActionResult<IEnumerable<EfficiencyByDate>> GetTotalCasesByQuarter(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var baseList = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            ).Where(x => filters.Lines!.Contains(x.Line)).ToList();

            var result = baseList.GroupBy(x => new { line = x.Line, quarter = GetQuarter(x.Date), year = x.Date.Year })
            .Select(g => new { quarter = g.Key.quarter, year = g.Key.year, line = g.Key.line, efficiency = g.Sum(ps => ps.Production) / g.Sum(ps => ps.MaxProduction) })
            .OrderBy(o => o.year)
            .ThenBy(o => o.quarter)
            .ToList();


            List<EfficiencyByDate> resultFormatted = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal efficiency = result[i].efficiency ?? 0;

                EfficiencyByDate aux = new()
                {
                    Time = result[i].quarter.ToString() + "/" + result[i].year.ToString(),
                    Type = result[i].line,
                    Value = Math.Round((efficiency * 100), 1)
                };

                resultFormatted.Add(aux);
            }

            return resultFormatted;
        }


        [HttpPost("efficiency-by-year")]
        public ActionResult<IEnumerable<EfficiencyByDate>> GetEfficiencyByYear(EfficiencyFilter filters)
        {
            string categories = filters.Categories.Count > 0 ? string.Join(",", filters.Categories) : string.Empty;
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = _PCLrepository.ExecuteStoredProcedure<DTOGetEfficiency>("GetEfficiency",
                new SqlParameter("@categories", categories),
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate)
            ).Where(ps => filters.Lines!.Contains(ps.Line))
            .GroupBy(x => new
            {
                x.Date.Year,
                x.Line
            })
            .Select(g => new
            {
                time = g.Key.Year,
                type = g.Key.Line,
                value = Convert.ToDecimal(Math.Round((decimal)(g.Sum(ps => ps.Production) / g.Sum(ps => ps.MaxProduction) * 100)!, 1)),
            })
            .OrderBy(y => y.time).ToList();

            return Ok(result);
        }


        [HttpPost("export-efficiencyline")]
        public async Task<IActionResult> ExportExcelEfficiencyPerLine(EfficiencyFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductionSummary
               .Where(ps => ps.Date >= startDate && ps.Date <= endDate && filters.Lines!.Contains(ps.Line))
               .GroupBy(ps => ps.Line)
               .Select(group => new
               {
                   Line = group.Key,
                   Production = group.Sum(ps => ps.Production),
                   MaxProduction = group.Sum(ps => ps.EffDen)
               })
               .ToListAsync();

            decimal? Production = result.Sum(s => s.Production);
            decimal? MaxProduction = result.Sum(s => s.MaxProduction);

            decimal? totalEff = (Production ?? 0) / (MaxProduction ?? 1);

            if (result.Count == 0)
            {
                return NotFound();
            }

            List<EfficiencyByLine> tempList = new();

            foreach (var item in result)
            {
                tempList.Add(new EfficiencyByLine
                {
                    Line = item.Line,
                    Efficiency = (item.Production ?? 0) / (item.MaxProduction ?? 1)
                });
            }

            DataTable sheet = new DataTable("Efficiency per Line");
            sheet.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Line"),
                new DataColumn("Efficiency"),
            });

            foreach (var row in tempList)
            {
                sheet.Rows.Add(
                row.Line,
                (row.Efficiency * 100).ToString("0.00") + "%"
                );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(sheet);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "reportEfficiency.xlsx");
                }
            }
        }

        [HttpPost("export-totalcasesLine")]
        public async Task<ActionResult<TotalCasesByLineFormatted>> ExportExcelCasesByLine(CasesAnalysisFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var data = await _context.ProductionSummary
            .Where(obj => obj.Date >= startDate && obj.Date <= endDate)
            .Where(l => filters.Lines.Contains(l.Line)).ToListAsync();

            var result = data.GroupBy(x => x.Line)
            .Select(g => new { Line = g.Key, Production = g.Sum(x => x.Production) }).ToList();

            if (result.Count == 0)
            {
                TotalCasesByLineFormatted abort = new();
                return abort;
            }

            int sumTotal = 0;
            List<TotalCasesByLine> tempList = new();

            for (int i = 0; i < result.Count; i++)
            {

                decimal production = result[i].Production ?? 0;
                sumTotal += Convert.ToInt32(production);
            }

            for (int i = 0; i < result.Count; i++)
            {

                decimal production = result[i].Production ?? 0;

                TotalCasesByLine aux = new()
                {
                    Line = result[i].Line,
                    Production = Convert.ToInt32(production),
                    Percent = Math.Round((production * 100) / sumTotal, 1)
                };
                tempList.Add(aux);
            }

            DataTable sheet = new DataTable("Cases per Line");
            sheet.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Line"),
                new DataColumn("Production"),
                new DataColumn("Percent"),
            });

            foreach (var row in tempList)
            {
                sheet.Rows.Add(
                row.Line,
                row.Production,
                row.Percent
                );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(sheet);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "reportCases.xlsx");
                }
            }

        }

    }
}