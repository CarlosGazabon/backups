using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using inventio.Models.DTO.Supervisor;
using inventio.Models.DTO;
using Inventio.Repositories.Formulas;
using Inventio.Utils.Formulas;
using inventio.Models.DTO.Efficiency;

namespace inventio.Repositories.Dashboards.SupervisorMetrics
{
    public class SupervisorRepository : ISupervisorRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IGeneralEfficiencyRepository _generalEfficiencyRepository;
        private readonly EfficiencyFormula _efficiencyFormula;

        public SupervisorRepository(ApplicationDBContext context, IGeneralEfficiencyRepository formulaEfficiency, EfficiencyFormula efficiencyFormula)
        {
            _context = context;
            _generalEfficiencyRepository = formulaEfficiency;
            _efficiencyFormula = efficiencyFormula;
        }

        private DTOSupervisorEffContainer GetSupervisorEff(SupervisorFilter filters, List<DTOEffDataSet> data)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = data
            .GroupBy(g => new
            {
                g.Supervisor,
            })
            .Select(g => new
            {
                g.Key.Supervisor,
                Production = g.Sum(s => s.Production),
                EffDen = g.Sum(s => s.EffDen),
                Efficiency = _efficiencyFormula.CalculateEfficiency(g.Sum(s => s.Production), g.Sum(s => s.EffDen), true, true, 1),
            })
            .ToList();

            List<DTOSupervisorEff> supervisorEffList = new();
            decimal totalProduction = 0;
            decimal totalEffDen = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalProduction += result[i].Production.GetValueOrDefault(0);
                totalEffDen += result[i].EffDen.GetValueOrDefault(0);

                decimal auxEff = result[i].Efficiency;

                DTOSupervisorEff aux = new()
                {
                    Supervisor = result[i].Supervisor,
                    EffPercentage = auxEff
                };

                supervisorEffList.Add(aux);
            }

            decimal totalEff = _efficiencyFormula.CalculateEfficiency(totalProduction, totalEffDen, true, true, 1);

            // sort  array by eff
            supervisorEffList = supervisorEffList.OrderByDescending(item => item.EffPercentage).ToList();

            DTOSupervisorEffContainer supervisorEffContainer = new()
            {
                EffPerSupervisor = supervisorEffList,
                TotalEff = totalEff
            };

            return supervisorEffContainer;
        }

        private DTOSupervisorProductionContainer GetSupervisorProduction(SupervisorFilter filters, List<DTOSupervisorDataSet> data)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var productionPerSupervisor = data
                .GroupBy(g => new
                {
                    g.Supervisor,
                })
                .Select(g => new DTOSupervisorProduction
                {
                    Supervisor = g.Key.Supervisor,
                    Production = g.Sum(s => s.Production),
                })
                .OrderByDescending(item => item.Production)
                .ToList();

            decimal? totalProduction = productionPerSupervisor.Sum(s => s.Production);

            DTOSupervisorProductionContainer supervisorProductionContainer = new()
            {
                ProductionPerSupervisor = productionPerSupervisor,
                TotalProduction = totalProduction.GetValueOrDefault(0)
            };

            return supervisorProductionContainer;
        }

        private DTOSupervisorHoursContainer GetSupervisorHours(SupervisorFilter filters, List<DTOEffDataSet> data)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var hoursPerSupervisor = data
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .GroupBy(g => new
                {
                    g.Supervisor
                })
                .Select(g => new DTOSupervisorHours
                {
                    Supervisor = g.Key.Supervisor,
                    Hours = g.Count(x => x.Flow == 1)
                })
                .OrderByDescending(item => item.Hours)
                .ToList();

            decimal totalHours = hoursPerSupervisor.Sum(s => s.Hours ?? 0);

            DTOSupervisorHoursContainer supervisorHoursContainer = new()
            {
                HoursPerSupervisor = hoursPerSupervisor,
                TotalHours = totalHours
            };

            return supervisorHoursContainer;
        }

        private DTOSupervisorScrapContainer GetSupervisorScraps(SupervisorFilter filters, List<DTOSupervisorDataSet> data)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = data
                .GroupBy(g => new
                {
                    g.Supervisor,
                })
                .Select(group => new
                {
                    group.Key.Supervisor,
                    ScrapUnits = group.Sum(ps => ps.ScrapUnits),
                    ScrapDen = group.Sum(ps => ps.ScrapDen) == 0 ? 1 : group.Sum(ps => ps.ScrapDen)
                })
                .ToList();

            List<DTOSupervisorScrap> supervisorScrapList = new();
            decimal totalScrapUnit = 0;
            decimal totalScrapDen = 0;

            for (int i = 0; i < result.Count; i++)
            {
                totalScrapUnit += result[i].ScrapUnits.GetValueOrDefault(0);
                totalScrapDen += result[i].ScrapDen.GetValueOrDefault(0);
                decimal? denominator = result[i].ScrapUnits + result[i].ScrapDen;

                decimal? totalScrapPercent = denominator == 0 ? result[i].ScrapUnits / 1 : result[i].ScrapUnits / denominator;

                DTOSupervisorScrap aux = new()
                {
                    Supervisor = result[i].Supervisor,
                    ScrapPercentage = Math.Round(totalScrapPercent.GetValueOrDefault(0) * 100, 2)
                };

                supervisorScrapList.Add(aux);
            }

            decimal totalDenominator = totalScrapUnit + totalScrapDen;
            decimal totalScrap = totalDenominator == 0 ? totalScrapUnit / 1 : totalScrapUnit / totalDenominator;

            // sort items by scrap percentage
            supervisorScrapList = supervisorScrapList.OrderByDescending(item => item.ScrapPercentage).ToList();

            DTOSupervisorScrapContainer supervisorScrapContainer = new()
            {
                ScrapPerSupervisor = supervisorScrapList,
                TotalScrap = Math.Round(totalScrap * 100, 2)
            };

            return supervisorScrapContainer;
        }

        // public methods
        public async Task<IEnumerable<DTOReactDropdown<int>>> GetLines(SupervisorFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwSupervisorMetrics
                .Where(w => w.Date >= startDate
                        && w.Date <= endDate)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Line,
                    Value = s.LineId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisors(SupervisorFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwSupervisorMetrics
                .Where(w => w.Date >= startDate
                        && w.Date <= endDate
                        && filters.Lines!.Contains(w.LineId))
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Supervisor,
                    Value = s.SupervisorId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetShifts(SupervisorFilter filters)
        {
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            var result = await _context.VwSupervisorMetrics
                .Where(w => w.Date >= startDate
                        && w.Date <= endDate
                        && filters.Lines!.Contains(w.LineId)
                        && filters.Supervisors.Contains(w.SupervisorId))
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Shift,
                    Value = s.ShiftId
                })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<DTOMetricsPerSupervisor> GetSupervisorDashBoard(SupervisorFilter filters)
        {
            // Final return
            DateTime startDate = DateTime.Parse(filters.StartDate);
            DateTime endDate = DateTime.Parse(filters.EndDate);

            //Test change over harcode
            var data = await _generalEfficiencyRepository.GetEfficiency(new DTOGetEfficiencyFilter
            {
                StartDate = filters.StartDate,
                EndDate = filters.EndDate,
                Categories = filters.Categories,
            });


            var effDataSet = data
           .Where(w => w.Date >= startDate
           && w.Date <= endDate
           && filters.Lines!.Contains(w.LineId)
           && filters.Supervisors.Contains(w.SupervisorId)
           && filters.Shifts.Contains(w.ShiftId))
           .Select(s => new DTOEffDataSet
           {
               Supervisor = s.Supervisor,
               Production = s.Production,
               EffDen = s.MaxProduction,
               Hours = s.Hrs,
               Flow = s.Flow,
               Date = s.Date,
               LineId = s.LineId,
           })
           .ToList();

            //TODO Original
            /* var effDataSet = await _context.VwGeneralEfficiency
           .Where(w => w.Date >= startDate
           && w.Date <= endDate
           && filters.Lines!.Contains(w.LineId)
           && filters.Supervisors.Contains(w.SupervisorId)
           && filters.Shifts.Contains(w.ShiftId))
           .Select(s => new DTOEffDataSet
           {
               Supervisor = s.Supervisor,
               Production = s.Production,
               EffDen = s.EffDen,
               Hours = s.Hrs,
               Flow = s.Flow,
               Date = s.Date,
               LineId = s.LineId,
           })
           .ToListAsync(); */

            var supervisorDataSet = await _context.VwSupervisorMetrics
                .Where(w => w.Date >= startDate
                && w.Date <= endDate
                && filters.Lines!.Contains(w.LineId)
                && filters.Supervisors.Contains(w.SupervisorId)
                && filters.Shifts.Contains(w.ShiftId))
                .GroupBy(g => new
                {
                    g.Supervisor,
                })
                .Select(group => new DTOSupervisorDataSet
                {
                    Supervisor = group.Key.Supervisor,
                    Production = group.Sum(s => s.Production),
                    ScrapUnits = group.Sum(s => s.ScrapUnits),
                    ScrapDen = group.Sum(s => s.ScrapDen) == 0 ? 1 : group.Sum(s => s.ScrapDen)
                })
                .ToListAsync();

            DTOSupervisorEffContainer supervisorEff = GetSupervisorEff(filters, effDataSet);
            DTOSupervisorProductionContainer supervisorProduction = GetSupervisorProduction(filters, supervisorDataSet);
            DTOSupervisorHoursContainer supervisorHours = GetSupervisorHours(filters, effDataSet);
            DTOSupervisorScrapContainer supervisorScrap = GetSupervisorScraps(filters, supervisorDataSet);

            DTOMetricsPerSupervisor resultFormatted = new()
            {
                Efficiency = supervisorEff,
                Production = supervisorProduction,
                Hours = supervisorHours,
                Scrap = supervisorScrap,
            };

            return resultFormatted;
        }
    }
}