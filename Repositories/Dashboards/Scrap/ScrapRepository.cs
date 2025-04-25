using Microsoft.EntityFrameworkCore;
using inventio.Utils;
using inventio.Models.DTO.ProductivitySummary;
using Inventio.Data;
using inventio.Models.DTO.ScrapController;
using inventio.Models.DTO;


namespace inventio.Repositories.Dashboards.Scrap
{
    public class ScrapRepository : IScrapRepository
    {

        private readonly ApplicationDBContext _context;

        public ScrapRepository(ApplicationDBContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<DTOReactDropdown<int>>> GetLines(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductionSummary
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Line,
                    Value = s.LineId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }
        public static int GetQuarter(DateTime date)
        {
            return (date.Month + 2) / 3;
        }



        public async Task<DTOScrapType> GetScrapAvailable()
        {
            var result = await _context.Line.Select(s => new DTOScrapType
            {
                BottleScrap = s.BottleScrap,
                CanScrap = s.CanScrap,
                PreformScrap = s.PreformScrap,
                PouchScrap = s.PouchScrap
            }).ToListAsync();

            DTOScrapType ScrapAvailables = new()
            {
                BottleScrap = false,
                CanScrap = false,
                PreformScrap = false,
                PouchScrap = false
            };


            foreach (var item in result)
            {
                if (item.BottleScrap == true)
                {
                    ScrapAvailables.BottleScrap = true;
                }

                if (item.CanScrap == true)
                {
                    ScrapAvailables.CanScrap = true;
                }

                if (item.PreformScrap == true)
                {
                    ScrapAvailables.PreformScrap = true;
                }

                if (item.PouchScrap == true)
                {
                    ScrapAvailables.PouchScrap = true;
                }
            }

            return ScrapAvailables;
        }

        public async Task<List<DTOShift>> GetScrapShift(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductionSummary
                .Where(w => filters.Lines.Contains(w.Line)
                && w.Date >= startDate && w.Date <= endDate)
                .GroupBy(g => g.Shift)
                .Select(s => new
                {
                    shift = s.Key
                })
                .ToListAsync();

            List<DTOShift> shiftList = new();

            foreach (var row in result)
            {
                DTOShift aux = new()
                {
                    Shift = row.shift
                };
                shiftList.Add(aux);
            }

            return shiftList;
        }

        public async Task<decimal> GetTotalPlantScrap(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
                .Where(ps => ps.Date >= startDate && ps.Date <= endDate)
                .GroupBy(ps => ps.Line)
                .Select(group => new
                {
                    Line = group.Key,
                    ScrapUnits = group.Sum(ps => ps.ScrapUnits),
                    ScrapDen = group.Sum(ps => ps.ScrapDEN) == 0 ? 1 : group.Sum(ps => ps.ScrapDEN)
                })
                .ToListAsync();

            int totalScrapUnits = result.Sum(s => s.ScrapUnits);
            decimal totalScrapDen = result.Sum(s => s.ScrapDen);

            decimal denominator = (totalScrapUnits + totalScrapDen) != 0 ? (totalScrapUnits + totalScrapDen) : 1;

            decimal totalScrapPercent = totalScrapUnits / denominator;

            return Math.Round(totalScrapPercent * 100, 2);

        }

        public async Task<List<DTOScrapTable>> GetScrapTableAllLines(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
                 .Where(ps => ps.Date >= startDate && ps.Date <= endDate)
                 .GroupBy(ps => new
                 {
                     ps.Line,
                 })
                 .Select(group => new
                 {
                     group.Key.Line,
                     PreformScrap = group.Sum(ps => ps.PreformScrap),
                     BottleScrap = group.Sum(ps => ps.BottleScrap),
                     CanScrap = group.Sum(ps => ps.CanScrap),
                     PouchScrap = group.Sum(ps => ps.PouchScrap),
                     ScrapUnits = group.Sum(ps => ps.ScrapUnits),
                     ScrapDen = group.Sum(ps => ps.ScrapDEN) == 0 ? 1 : group.Sum(ps => ps.ScrapDEN)
                 })
                 .ToListAsync();

            List<DTOScrapTable> scrapList = new();


            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapTable aux = new()
                {
                    Line = result[i].Line,
                    ScrapDEN = result[i].ScrapDen,
                    PreformScrap = result[i].PreformScrap,
                    BottleScrap = result[i].BottleScrap,
                    CanScrap = result[i].CanScrap,
                    PouchScrap = result[i].PouchScrap,
                    TotalScrap = result[i].ScrapUnits,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;
        }

        private async Task<List<DTOScrapPerLine>> GetScrapPerLine(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
                .Where(ps => ps.Date >= startDate && ps.Date <= endDate && filters.Lines!.Contains(ps.Line) && filters.Shifts!.Contains(ps.Shift))
                .GroupBy(ps => ps.Line)
                .Select(group => new
                {
                    Line = group.Key,
                    PreformScrap = group.Sum(ps => ps.PreformScrap),
                    BottleScrap = group.Sum(ps => ps.BottleScrap),
                    CanScrap = group.Sum(ps => ps.CanScrap),
                    PouchScrap = group.Sum(ps => ps.PouchScrap),
                    ScrapDen = group.Sum(ps => ps.ScrapDEN) == 0 ? 1 : group.Sum(ps => ps.ScrapDEN)
                })
                .ToListAsync();

            List<DTOScrapPerLine> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {

                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);


                DTOScrapPerLine auxPreform = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Preform",
                    ScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxBottle = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Bottle",
                    ScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxCan = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Can",
                    ScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                };

                DTOScrapPerLine auxPouch = new()
                {
                    Line = result[i].Line,
                    ScrapType = "Pouch",
                    ScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                };

                scrapList.Add(auxPreform);
                scrapList.Add(auxBottle);
                scrapList.Add(auxCan);
                scrapList.Add(auxPouch);
            }

            return scrapList;

        }

        private async Task<List<DTOScrapTable>> GetScrapTable(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
                 .Where(ps => ps.Date >= startDate && ps.Date <= endDate && filters.Lines!.Contains(ps.Line) && filters.Shifts!.Contains(ps.Shift))
                 .GroupBy(ps => new
                 {
                     ps.Line,
                 })
                 .Select(group => new
                 {
                     group.Key.Line,
                     PreformScrap = group.Sum(ps => ps.PreformScrap),
                     BottleScrap = group.Sum(ps => ps.BottleScrap),
                     CanScrap = group.Sum(ps => ps.CanScrap),
                     PouchScrap = group.Sum(ps => ps.PouchScrap),
                     ScrapUnits = group.Sum(ps => ps.ScrapUnits),
                     ScrapDen = group.Sum(ps => ps.ScrapDEN) == 0 ? 1 : group.Sum(ps => ps.ScrapDEN)
                 })
                 .ToListAsync();

            List<DTOScrapTable> scrapList = new();


            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapTable aux = new()
                {
                    Line = result[i].Line,
                    ScrapDEN = result[i].ScrapDen,
                    PreformScrap = result[i].PreformScrap,
                    BottleScrap = result[i].BottleScrap,
                    CanScrap = result[i].CanScrap,
                    PouchScrap = result[i].PouchScrap,
                    TotalScrap = result[i].ScrapUnits,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;
        }

        //* --- SCRAP COLUMN PER TIME ---

        private async Task<List<DTOScrapPerTime>> GetScrapColumnByDay(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
            .Where(w => filters.Lines!.Contains(w.Line)
            && w.Date >= startDate && w.Date <= endDate
            && filters.Shifts!.Contains(w.Shift))
            .GroupBy(g => new
            {
                g.Date,
                g.Line,
            })
            .Select(s => new
            {
                s.Key.Date,
                s.Key.Line,
                PreformScrap = s.Sum(ps => ps.PreformScrap),
                BottleScrap = s.Sum(ps => ps.BottleScrap),
                CanScrap = s.Sum(ps => ps.CanScrap),
                PouchScrap = s.Sum(ps => ps.PouchScrap),
                ScrapUnits = s.Sum(ps => ps.ScrapUnits),
                ScrapDen = s.Sum(ps => ps.ScrapDEN) == 0 ? 1 : s.Sum(ps => ps.ScrapDEN)
            })
            .OrderBy(o => o.Date)
            .ToListAsync();


            List<DTOScrapPerTime> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapPerTime aux = new()
                {
                    Time = result[i].Date.ToString("MM/dd/yy"),
                    Line = result[i].Line,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;

        }

        private async Task<List<DTOScrapPerTime>> GetScrapColumnByWeek(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            Charts charts = new();

            var baseList = await _context.ProductivitySummary
                         .Where(w => filters.Lines!.Contains(w.Line)
                         && w.Date >= startDate && w.Date <= endDate
                         && filters.Shifts!.Contains(w.Shift)).ToListAsync();

            var result = baseList.GroupBy(g => new
            {
                Week = charts.GetIso8601WeekOfYear(g.Date),
                g.Date.Year,
                g.Line,
            })
            .Select(s => new
            {
                s.Key.Week,
                s.Key.Year,
                s.Key.Line,
                PreformScrap = s.Sum(ps => ps.PreformScrap),
                BottleScrap = s.Sum(ps => ps.BottleScrap),
                CanScrap = s.Sum(ps => ps.CanScrap),
                PouchScrap = s.Sum(ps => ps.PouchScrap),
                ScrapUnits = s.Sum(ps => ps.ScrapUnits),
                ScrapDen = s.Sum(ps => ps.ScrapDEN) == 0 ? 1 : s.Sum(ps => ps.ScrapDEN)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Week).ToList();


            List<DTOScrapPerTime> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapPerTime aux = new()
                {
                    Time = $"{result[i].Week}/{result[i].Year}",
                    Line = result[i].Line,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;
        }

        private async Task<List<DTOScrapPerTime>> GetScrapColumnByMonth(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
              .Where(w => filters.Lines!.Contains(w.Line)
              && w.Date >= startDate && w.Date <= endDate
              && filters.Shifts!.Contains(w.Shift))
            .GroupBy(g => new
            {
                g.Date.Month,
                g.Date.Year,
                g.Line,
            })
            .Select(s => new
            {
                s.Key.Month,
                s.Key.Year,
                s.Key.Line,
                PreformScrap = s.Sum(ps => ps.PreformScrap),
                BottleScrap = s.Sum(ps => ps.BottleScrap),
                CanScrap = s.Sum(ps => ps.CanScrap),
                PouchScrap = s.Sum(ps => ps.PouchScrap),
                ScrapUnits = s.Sum(ps => ps.ScrapUnits),
                ScrapDen = s.Sum(ps => ps.ScrapDEN) == 0 ? 1 : s.Sum(ps => ps.ScrapDEN)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Month).ToListAsync();


            List<DTOScrapPerTime> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapPerTime aux = new()
                {
                    Time = $"{result[i].Month}/{result[i].Year}",
                    Line = result[i].Line,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;

        }

        private async Task<List<DTOScrapPerTime>> GetScrapColumnByQuarter(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            Charts charts = new();

            var baseList = await _context.ProductivitySummary
                         .Where(w => filters.Lines!.Contains(w.Line)
                         && w.Date >= startDate && w.Date <= endDate
                         && filters.Shifts!.Contains(w.Shift)).ToListAsync();

            var result = baseList.GroupBy(g => new
            {
                Quarter = (g.Date.Month - 1) / 3 + 1,
                g.Date.Year,
                g.Line,
            })
            .Select(s => new
            {
                s.Key.Quarter,
                s.Key.Year,
                s.Key.Line,
                PreformScrap = s.Sum(ps => ps.PreformScrap),
                BottleScrap = s.Sum(ps => ps.BottleScrap),
                CanScrap = s.Sum(ps => ps.CanScrap),
                PouchScrap = s.Sum(ps => ps.PouchScrap),
                ScrapUnits = s.Sum(ps => ps.ScrapUnits),
                ScrapDen = s.Sum(ps => ps.ScrapDEN) == 0 ? 1 : s.Sum(ps => ps.ScrapDEN)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Quarter).ToList();


            List<DTOScrapPerTime> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapPerTime aux = new()
                {
                    Time = $"{result[i].Quarter}/{result[i].Year}",
                    Line = result[i].Line,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;
        }

        private async Task<List<DTOScrapPerTime>> GetScrapColumnByYear(ScrapFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.ProductivitySummary
              .Where(w => filters.Lines!.Contains(w.Line)
              && w.Date >= startDate && w.Date <= endDate
              && filters.Shifts!.Contains(w.Shift))
            .GroupBy(g => new
            {
                g.Date.Year,
                g.Line,
            })
            .Select(s => new
            {
                s.Key.Year,
                s.Key.Line,
                PreformScrap = s.Sum(ps => ps.PreformScrap),
                BottleScrap = s.Sum(ps => ps.BottleScrap),
                CanScrap = s.Sum(ps => ps.CanScrap),
                PouchScrap = s.Sum(ps => ps.PouchScrap),
                ScrapUnits = s.Sum(ps => ps.ScrapUnits),
                ScrapDen = s.Sum(ps => ps.ScrapDEN) == 0 ? 1 : s.Sum(ps => ps.ScrapDEN)
            })
            .OrderBy(o => o.Year)
            .ToListAsync();


            List<DTOScrapPerTime> scrapList = new();

            for (int i = 0; i < result.Count; i++)
            {
                decimal totalScrapPercent = result[i].ScrapUnits / (result[i].ScrapUnits + result[i].ScrapDen);
                decimal preformScrapPercent = result[i].PreformScrap / (result[i].PreformScrap + result[i].ScrapDen);
                decimal bottleScrapPercent = result[i].BottleScrap / (result[i].BottleScrap + result[i].ScrapDen);
                decimal canScrapPercent = result[i].CanScrap / (result[i].CanScrap + result[i].ScrapDen);
                decimal pouchScrapPercent = result[i].PouchScrap / (result[i].PouchScrap + result[i].ScrapDen);

                DTOScrapPerTime aux = new()
                {
                    Time = result[i].Year.ToString(),
                    Line = result[i].Line,
                    PreformScrapPercentage = Math.Round(preformScrapPercent * 100, 2),
                    BottleScrapPercentage = Math.Round(bottleScrapPercent * 100, 2),
                    CanScrapPercentage = Math.Round(canScrapPercent * 100, 2),
                    PouchScrapPercentage = Math.Round(pouchScrapPercent * 100, 2),
                    TotalScrapPercentage = Math.Round(totalScrapPercent * 100, 2)
                };

                scrapList.Add(aux);
            }

            return scrapList;

        }

        //* --- END SCRAP COLUMN PER TIME ---

        public async Task<DTOScrapDashboard> GetScrapDashboard(ScrapFilter filters)
        {
            var totalPlant = await GetTotalPlantScrap(filters);
            var scrapTable = await GetScrapTable(filters);
            var scrapPerLine = await GetScrapPerLine(filters);
            List<DTOScrapPerTime> scrapPerTime = new();

            switch (filters.Time)
            {
                case "Day":
                    scrapPerTime = await GetScrapColumnByDay(filters);
                    break;
                case "Week":
                    scrapPerTime = await GetScrapColumnByWeek(filters);
                    break;
                case "Month":
                    scrapPerTime = await GetScrapColumnByMonth(filters);
                    break;
                case "Quarter":
                    scrapPerTime = await GetScrapColumnByQuarter(filters);
                    break;
                case "Year":
                    scrapPerTime = await GetScrapColumnByYear(filters);
                    break;
            }

            DTOScrapDashboard resultTrend = new()
            {
                PlantScrapPercentage = totalPlant,
                ScrapTable = scrapTable,
                ScrapPerLine = scrapPerLine,
                ScrapPerTime = scrapPerTime
            };

            return resultTrend;
        }
    }
}