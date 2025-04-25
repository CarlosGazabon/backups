using Microsoft.AspNetCore.Mvc;
using inventio.Models.DTO.Report;
using inventio.Repositories.ShiftProductionReport;
using inventio.Repositories.Reports.SummaryReport;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PDFController : ControllerBase
    {
        private readonly IShiftProductionReportRepository _shiftProductionReportRepository;
        private readonly ISummaryReport _summaryReportRepository;

        public PDFController(IShiftProductionReportRepository shiftProductionReportRepository, ISummaryReport summaryReportRepository)
        {
            _shiftProductionReportRepository = shiftProductionReportRepository;
            _summaryReportRepository = summaryReportRepository;
        }

        [HttpPost("shift-production")]
        public IActionResult CreatePDFShiftProduction([FromBody] ReportFilter request)
        {
            try
            {
                var fileContentResult = _shiftProductionReportRepository.CreatePDFShiftProduction(request);
                return File(fileContentResult, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("summary-production")]
        public async Task<IActionResult> CreatePDFSummaryProduction([FromBody] SummaryReportFilter request)
        {
            try
            {
                var fileContentResult = await _summaryReportRepository.CreatePDFSummaryProduction(request);

                return File(fileContentResult, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}