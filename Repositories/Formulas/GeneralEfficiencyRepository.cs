using inventio.Models.DTO.Efficiency;
using inventio.Models.StoredProcedure;
using Inventio.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Formulas
{
  public class GeneralEfficiencyRepository : IGeneralEfficiencyRepository
  {
    private readonly ApplicationDBContext _context;

    public GeneralEfficiencyRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    //* --------------------------------- Metodos -------------------------------- */
    public async Task<List<SPGetDataEfficiency>> GetEfficiency(DTOGetEfficiencyFilter filters)
    {
      filters.Categories ??= new List<int>();

      string categories = string.Join(",", filters.Categories);
      DateTime startDate = DateTime.Parse(filters.StartDate);
      DateTime endDate = DateTime.Parse(filters.EndDate);

      var data = await _context.GetDataEfficiency
        .FromSqlRaw("EXEC GetDataEfficiency @categories, @startDate, @endDate",
          new SqlParameter("@categories", categories),
          new SqlParameter("@startDate", startDate),
          new SqlParameter("@endDate", endDate))
        .ToListAsync();

      return data;
    }
  }
}