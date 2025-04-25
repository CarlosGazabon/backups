using inventio.Models.DTO;
using inventio.Models.DTO.DowntimeTrend;
using inventio.Utils;
using Inventio.Data;
using Inventio.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Dashboards.TrendAnalysisRepository
{
    public class TrendAnalysisRepository : ITrendAnalysisRepository
    {

        private readonly ApplicationDBContext _context;

        public TrendAnalysisRepository(ApplicationDBContext context)
        {
            _context = context;
        }


        //******************** PRIVATE METHODS ***********************************************************
        //* TREND ANALYSIS TABLE PER TIME


        private DTOTrendTableSet GetTrendTableByDay(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
             .GroupBy(g => new
             {
                 g.Date,
                 g.Category,
                 g.Line,
                 g.SubCategory2,
                 g.Code,
                 g.Failure
             })
             .Select(s => new
             {
                 s.Key.Date,
                 s.Key.Category,
                 s.Key.Line,
                 s.Key.SubCategory2,
                 s.Key.Code,
                 s.Key.Failure,
                 Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
             })
             .OrderBy(o => o.Date)
             .ToList();


            List<DTOTrendTable> trendList = new();
            decimal totalMinutes = 0;

            for (int i = 0; i < result.Count; i++)
            {

                totalMinutes += result[i].Minutes;

                DTOTrendTable aux = new()
                {
                    Date = result[i].Date.ToString("MM/dd/yy"),
                    Category = result[i].Category,
                    Line = result[i].Line,
                    SubCategory2 = result[i].SubCategory2,
                    Failure = $"{result[i].Code} | {result[i].Failure}",
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return new DTOTrendTableSet()
            {
                TotalMinutes = totalMinutes,
                TrendTable = trendList
            };


        }

        public DTOTrendTableSet GetTrendTableByWeek(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            Charts charts = new();

            var result = baseList.GroupBy(g => new
            {
                Week = charts.GetIso8601WeekOfYear(g.Date),
                g.Date.Year,
                g.Category,
                g.Line,
                g.SubCategory2,
                g.Code,
                g.Failure
            })
            .Select(s => new
            {
                s.Key.Week,
                s.Key.Year,
                s.Key.Category,
                s.Key.Line,
                s.Key.SubCategory2,
                s.Key.Code,
                s.Key.Failure,
                Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Week).ToList();


            List<DTOTrendTable> trendList = new();
            decimal totalMinutes = 0;

            for (int i = 0; i < result.Count; i++)
            {

                totalMinutes += result[i].Minutes;

                DTOTrendTable aux = new()
                {
                    Date = $"{result[i].Week}/{result[i].Year}",
                    Category = result[i].Category,
                    Line = result[i].Line,
                    SubCategory2 = result[i].SubCategory2,
                    Failure = $"{result[i].Code} | {result[i].Failure}",
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return new DTOTrendTableSet()
            {
                TotalMinutes = totalMinutes,
                TrendTable = trendList
            };
        }

        public DTOTrendTableSet GetTrendTableByMonth(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
                        .GroupBy(g => new
                        {
                            g.Date.Month,
                            g.Date.Year,
                            g.Category,
                            g.Line,
                            g.SubCategory2,
                            g.Code,
                            g.Failure
                        })
                        .Select(s => new
                        {
                            s.Key.Month,
                            s.Key.Year,
                            s.Key.Category,
                            s.Key.Line,
                            s.Key.SubCategory2,
                            s.Key.Code,
                            s.Key.Failure,
                            Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
                        })
                        .OrderBy(o => o.Year)
                        .ThenBy(m => m.Month).ToList();


            List<DTOTrendTable> trendList = new();
            decimal totalMinutes = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalMinutes += result[i].Minutes;

                DTOTrendTable aux = new()
                {
                    Date = $"{result[i].Month}/{result[i].Year}",
                    Category = result[i].Category,
                    Line = result[i].Line,
                    SubCategory2 = result[i].SubCategory2,
                    Failure = $"{result[i].Code} | {result[i].Failure}",
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return new DTOTrendTableSet()
            {
                TotalMinutes = totalMinutes,
                TrendTable = trendList
            };

        }

        public DTOTrendTableSet GetTrendTableByQuarter(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            Charts charts = new();

            var result = baseList.GroupBy(g => new
            {
                Quarter = charts.GetQuarter(g.Date),
                g.Date.Year,
                g.Category,
                g.Line,
                g.SubCategory2,
                g.Code,
                g.Failure
            })
            .Select(s => new
            {
                s.Key.Quarter,
                s.Key.Year,
                s.Key.Category,
                s.Key.Line,
                s.Key.SubCategory2,
                s.Key.Code,
                s.Key.Failure,
                Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Quarter).ToList();


            List<DTOTrendTable> trendList = new();
            decimal totalMinutes = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalMinutes += result[i].Minutes;

                DTOTrendTable aux = new()
                {
                    Date = $"{result[i].Quarter}/{result[i].Year}",
                    Category = result[i].Category,
                    Line = result[i].Line,
                    SubCategory2 = result[i].SubCategory2,
                    Failure = $"{result[i].Code} | {result[i].Failure}",
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return new DTOTrendTableSet()
            {
                TotalMinutes = totalMinutes,
                TrendTable = trendList
            };
        }

        public DTOTrendTableSet GetTrendTableByYear(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
                        .GroupBy(g => new
                        {
                            g.Date.Year,
                            g.Category,
                            g.Line,
                            g.SubCategory2,
                            g.Code,
                            g.Failure
                        })
                        .Select(s => new
                        {
                            s.Key.Year,
                            s.Key.Category,
                            s.Key.Line,
                            s.Key.SubCategory2,
                            s.Key.Code,
                            s.Key.Failure,
                            Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
                        })
                        .OrderBy(o => o.Year)
                        .ToList();


            List<DTOTrendTable> trendList = new();
            decimal totalMinutes = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalMinutes += result[i].Minutes;

                DTOTrendTable aux = new()
                {
                    Date = result[i].Year.ToString(),
                    Category = result[i].Category,
                    Line = result[i].Line,
                    SubCategory2 = result[i].SubCategory2,
                    Failure = $"{result[i].Code} | {result[i].Failure}",
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return new DTOTrendTableSet()
            {
                TotalMinutes = totalMinutes,
                TrendTable = trendList
            };

        }

        //* END TREND ANALYSIS TABLE PER TIME

        //* TREND ANALYSIS COLUMN PER TIME

        public List<DTOTrendColumnChart> GetTrendColumnByDay(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
                        .GroupBy(g => new
                        {
                            g.Date,
                            g.SubCategory2,
                        })
                        .Select(s => new
                        {
                            s.Key.Date,
                            s.Key.SubCategory2,
                            Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
                        })
                        .OrderBy(o => o.Date)
                        .ToList();


            List<DTOTrendColumnChart> trendList = new();

            for (int i = 0; i < result.Count; i++)
            {
                DTOTrendColumnChart aux = new()
                {
                    Date = result[i].Date.ToString("MM/dd/yy"),
                    SubCategory2 = result[i].SubCategory2,
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return trendList;

        }

        public List<DTOTrendColumnChart> GetTrendColumnByWeek(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {
            Charts charts = new();

            var result = baseList.GroupBy(g => new
            {
                Week = charts.GetIso8601WeekOfYear(g.Date),
                g.Date.Year,
                g.SubCategory2,
            })
            .Select(s => new
            {
                s.Key.Week,
                s.Key.Year,
                s.Key.SubCategory2,
                Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Week).ToList();


            List<DTOTrendColumnChart> trendList = new();

            for (int i = 0; i < result.Count; i++)
            {
                DTOTrendColumnChart aux = new()
                {
                    Date = $"{result[i].Week}/{result[i].Year}",
                    SubCategory2 = result[i].SubCategory2,
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return trendList;
        }

        public List<DTOTrendColumnChart> GetTrendColumnByMonth(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
                        .GroupBy(g => new
                        {
                            g.Date.Month,
                            g.Date.Year,
                            g.SubCategory2,
                        })
                        .Select(s => new
                        {
                            s.Key.Month,
                            s.Key.Year,
                            s.Key.SubCategory2,
                            Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
                        })
                        .OrderBy(o => o.Year)
                        .ThenBy(m => m.Month).ToList();


            List<DTOTrendColumnChart> trendList = new();

            for (int i = 0; i < result.Count; i++)
            {
                DTOTrendColumnChart aux = new()
                {
                    Date = $"{result[i].Month}/{result[i].Year}",
                    SubCategory2 = result[i].SubCategory2,
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return trendList;

        }

        public List<DTOTrendColumnChart> GetTrendColumnByQuarter(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            Charts charts = new();

            var result = baseList.GroupBy(g => new
            {
                Quarter = charts.GetQuarter(g.Date),
                g.Date.Year,
                g.SubCategory2,
            })
            .Select(s => new
            {
                s.Key.Quarter,
                s.Key.Year,
                s.Key.SubCategory2,
                Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
            })
            .OrderBy(o => o.Year)
            .ThenBy(m => m.Quarter).ToList();


            List<DTOTrendColumnChart> trendList = new();

            for (int i = 0; i < result.Count; i++)
            {
                DTOTrendColumnChart aux = new()
                {
                    Date = $"{result[i].Quarter}/{result[i].Year}",
                    SubCategory2 = result[i].SubCategory2,
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return trendList;
        }

        public List<DTOTrendColumnChart> GetTrendColumnByYear(DTOTrendFilter filters, List<VwDowntimeTrend> baseList)
        {

            var result = baseList
                        .GroupBy(g => new
                        {
                            g.Date.Year,
                            g.SubCategory2,
                        })
                        .Select(s => new
                        {
                            s.Key.Year,
                            s.Key.SubCategory2,
                            Minutes = s.Sum(u => u.Minutes) + s.Sum(u => u.ExtraMinutes)
                        })
                        .OrderBy(o => o.Year)
                        .ToList();


            List<DTOTrendColumnChart> trendList = new();

            for (int i = 0; i < result.Count; i++)
            {
                DTOTrendColumnChart aux = new()
                {
                    Date = result[i].Year.ToString(),
                    SubCategory2 = result[i].SubCategory2,
                    Minutes = result[i].Minutes
                };

                trendList.Add(aux);
            }

            return trendList;

        }
        //* END TREND ANALYSIS COLUMN PER TIME
        //******************** END PRIVATE METHODS *******************************************************

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetLinesAsync(DTOTrendFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeTrend
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

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetCategoriesAsync(DTOTrendFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeTrend
                .Where(w => filters.Lines.Contains(w.LineId) && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Category,
                    Value = s.CategoryId
                })
                .Distinct()
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetSubCategoryAsync(DTOTrendFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeTrend
                .Where(w => filters.Lines.Contains(w.LineId)
                    && filters.Categories.Contains(w.CategoryId)
                    && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.SubCategory2,
                    Value = s.SubCategory2Id
                })
                .Distinct()
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetCodesAsync(DTOTrendFilter filters)
        {

            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwDowntimeTrend
                .Where(w => filters.Lines.Contains(w.LineId)
                    && filters.Categories.Contains(w.CategoryId)
                    && filters.SubCategories.Contains(w.SubCategory2Id)
                    && w.Date >= startDate && w.Date <= endDate)
                .Select(s => new
                {
                    s.Code,
                    s.Failure,
                    s.CodeId
                })
                .Distinct()
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = $"{s.Code} - {s.Failure}",
                    Value = s.CodeId
                })
                .ToListAsync();


            return result;
        }

        public async Task<DTODowntimeTrendDashboard> GetTrendDashBoard(DTOTrendFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            List<VwDowntimeTrend> baseList = await _context.VwDowntimeTrend
                        .Where(w => filters.Lines.Contains(w.LineId)
                        && filters.SubCategories.Contains(w.SubCategory2Id)
                        && filters.Codes.Contains(w.CodeId)
                        && w.Date >= startDate && w.Date <= endDate).ToListAsync();

            DTOTrendTableSet trendTableSet = new();
            List<DTOTrendColumnChart> trendColumnPerTime = new();

            switch (filters.Time)
            {
                case "Day":
                    trendTableSet = GetTrendTableByDay(filters, baseList);
                    trendColumnPerTime = GetTrendColumnByDay(filters, baseList);
                    break;
                case "Week":
                    trendTableSet = GetTrendTableByWeek(filters, baseList);
                    trendColumnPerTime = GetTrendColumnByWeek(filters, baseList);
                    break;
                case "Month":
                    trendTableSet = GetTrendTableByMonth(filters, baseList);
                    trendColumnPerTime = GetTrendColumnByMonth(filters, baseList);
                    break;
                case "Quarter":
                    trendTableSet = GetTrendTableByQuarter(filters, baseList);
                    trendColumnPerTime = GetTrendColumnByQuarter(filters, baseList);
                    break;
                case "Year":
                    trendTableSet = GetTrendTableByYear(filters, baseList);
                    trendColumnPerTime = GetTrendColumnByYear(filters, baseList);
                    break;
            }

            DTODowntimeTrendDashboard resultTrend = new()
            {
                TotalTableMinutes = trendTableSet.TotalMinutes,
                TrendTable = trendTableSet.TrendTable,
                TrendColumn = trendColumnPerTime
            };

            return resultTrend;

        }
    }
}