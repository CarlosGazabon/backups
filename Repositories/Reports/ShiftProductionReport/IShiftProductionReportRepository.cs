using inventio.Models.DTO.Report;
namespace inventio.Repositories.ShiftProductionReport
{
    public interface IShiftProductionReportRepository
    {
        byte[] CreatePDFShiftProduction(ReportFilter request);
    }
}