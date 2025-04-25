using inventio.Models.DTO;
using inventio.Models.DTO.Productivity;
using Inventio.Models.DTO.Productivity;

namespace inventio.Repositories.Records.Production
{
  public interface IProductionRepository
  {
    Task<IEnumerable<DTOReactDropdown<int>>> GetLines(ProductivityFilter filters);
    Task<IEnumerable<DTOReactDropdown<int>>> GetSupervisor(ProductivityFilter filters);
    Task<IEnumerable<DTOReactDropdown<int>>> GetSku(ProductivityFilter filters);
    Task<IEnumerable<DTOReactDropdown<int>>> GetShift(ProductivityFilter filters);
    Task<DTOResultRecords> GetProductivityRecords(ProductivityFilter filters);
  }
}