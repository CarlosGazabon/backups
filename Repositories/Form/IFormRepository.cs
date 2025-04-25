using Inventio.Models;
using Inventio.Models.DTO.Form;

namespace Inventio.Repositories.Form
{
  public interface IFormRepository
  {
    Task<DTOSelectOptions> GetSelectOptions();
    Task<DTOProductByLine> GetProductByLine(int idLine);

    Task<List<DowntimeCategory>> GetCategory();

    Task<List<DowntimeSubCategory1>> GetSubcategory1(int idCategory);

    Task<List<DowntimeSubCategory2>> GetSubcategory2(int idSubcategory1);

    Task<List<DowntimeCode>> GetCodes(int idSubcategory2);

    Task<Productivity> CreateRecord(Productivity record);

    Task<Productivity> GetFormById(int id);

    Task UpdateRecord(int id, Productivity record);
  }
}