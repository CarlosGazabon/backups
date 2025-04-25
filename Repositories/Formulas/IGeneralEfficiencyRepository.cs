using inventio.Models.StoredProcedure;
using inventio.Models.DTO.Efficiency;

namespace Inventio.Repositories.Formulas
{
  public interface IGeneralEfficiencyRepository
  {
    Task<List<SPGetDataEfficiency>> GetEfficiency(DTOGetEfficiencyFilter filters);
  }
}