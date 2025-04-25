using Microsoft.AspNetCore.Mvc;
using inventio.Models.DTO.VwUtilization;
using inventio.Models.DTO;
using ClosedXML.Excel;
using System.Data;
using inventio.Repositories.Dashboards.Utilization;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/utilization")]
    public class UtilizationController : ControllerBase
    {
        private readonly IUtilizationRepository _utilizationRepository;

        public UtilizationController(IUtilizationRepository utilizationRepository)
        {
            _utilizationRepository = utilizationRepository;
        }

        [HttpPost("dashboard")]
        public async Task<ActionResult<DTOUtilizationDashboard>> GetUtilizationDashBoard(UtilizationFilter filters)
        {
            var plantUtilization = await _utilizationRepository.GetPlantUtilization(filters);
            var utilizationPerLine = await _utilizationRepository.GetUtilizationPerLine(filters);
            var utilizationTable = await _utilizationRepository.GetUtilizationTable(filters);
            List<DTOUtilizationPerTime> utilizationPerTime = new();

            switch (filters.Time)
            {
                case "Day":
                    utilizationPerTime = await _utilizationRepository.GetUtilizationPerDay(filters);
                    break;
                case "Week":
                    utilizationPerTime = await _utilizationRepository.GetUtilizationPerWeek(filters);
                    break;
                case "Month":
                    utilizationPerTime = await _utilizationRepository.GetUtilizationPerMonth(filters);
                    break;
                case "Quarter":
                    utilizationPerTime = await _utilizationRepository.GetUtilizationPerQuarter(filters);
                    break;
                case "Year":
                    utilizationPerTime = await _utilizationRepository.GetUtilizationPerYear(filters);
                    break;
            }

            DTOUtilizationDashboard resultUtilization = new()
            {
                PlantUtilization = plantUtilization,
                UtilizationPerLine = utilizationPerLine,
                UtilizationTable = utilizationTable,
                UtilizationPerTime = utilizationPerTime
            };

            return Ok(resultUtilization);
        }

        [HttpPost("export-utilizationtable")]
        public async Task<ActionResult> ExportExcelUtilizationTable(UtilizationFilter filters)
        {
            var utilizationTable = await _utilizationRepository.GetUtilizationTable(filters);

            DataTable sheet = new DataTable("Utilization Detail per Line");
            sheet.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Line"),
                new DataColumn("Utilization"),
                new DataColumn("Change over"),
                new DataColumn("Production"),
                new DataColumn("Total"),
            });

            foreach (var row in utilizationTable)
            {
                sheet.Rows.Add(
                    row.Line,
                    row.Utilization.ToString("0.00") + "%",
                    row.ChangeHrs.ToString(),
                    row.NetHrs.ToString(),
                    row.TotalHrs.ToString()
                );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.Worksheets.Add(sheet);
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "reportUtilizationTable.xlsx");
                }
            }
        }

        [HttpPost("lines")]
        public async Task<ActionResult<DTOReactDropdown<int>>> GetUtilizationLines(UtilizationFilter filters)
        {
            var result = await _utilizationRepository.GetUtilizationLines(filters);
            return Ok(result);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<DTOReactDropdown<string>>> GetUtilizationCategories(UtilizationFilter filters)
        {
            var result = await _utilizationRepository.GetUtilizationCategories(filters);
            return Ok(result);
        }
    }
}
