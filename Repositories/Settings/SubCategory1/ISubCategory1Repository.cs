using inventio.Models.DTO;
using inventio.Models.DTO.Settings.SubCategory1;

namespace inventio.Repositories.Settings.SubCategory1
{
    public interface ISubCategory1Repository
    {
        Task<IEnumerable<DTOReactDropdown<int>>> GetDowntimeSubCategoriesByCategoryAsync(int categoryId);
        Task<IEnumerable<SubCategoryWithCategoryDTO>> GetSubCategoriesWithCategoryAsync(int categoryId);
        Task<IEnumerable<SubCategoryWithCategoryDTO>> GetSubCategoriesWithCategoryAndSubCategoryAsync(int categoryId, int subCategoryId);

    }
}