using inventio.Models.DTO.Report;

namespace inventio.Repositories.Reports.SummaryReport
{
    public interface ISummaryReport
    {
        Task<byte[]> CreatePDFSummaryProduction(SummaryReportFilter request);
    }
}