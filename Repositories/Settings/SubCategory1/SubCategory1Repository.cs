using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using inventio.Models.DTO;
using inventio.Models.DTO.Settings.SubCategory1;
using Inventio.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace inventio.Repositories.Settings.SubCategory1
{
    public class SubCategory1Repository : ISubCategory1Repository
    {

        private readonly ApplicationDBContext _context;

        public SubCategory1Repository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DTOReactDropdown<int>>> GetDowntimeSubCategoriesByCategoryAsync(int categoryId)
        {
            return await _context.DowntimeSubCategory1
                .Where(w => w.DowntimeCategoryID == categoryId)
                .Select(s => new DTOReactDropdown<int>
                {
                    Label = s.Name,
                    Value = s.Id
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SubCategoryWithCategoryDTO>> GetSubCategoriesWithCategoryAsync(int categoryId)
        {
            return await _context.DowntimeSubCategory1
                .Where(sc => sc.DowntimeCategoryID == categoryId)
                .Select(sc => new SubCategoryWithCategoryDTO
                {
                    SubCategory1Id = sc.Id,
                    SubCategory1 = sc.Name,
                    Category = sc.DowntimeCategory.Name,
                    Inactive = sc.Inactive
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SubCategoryWithCategoryDTO>> GetSubCategoriesWithCategoryAndSubCategoryAsync(int categoryId, int subCategoryId)
        {
            return await _context.DowntimeSubCategory1
                .Where(sc => sc.DowntimeCategoryID == categoryId && sc.Id == subCategoryId)
                .Select(sc => new SubCategoryWithCategoryDTO
                {
                    SubCategory1Id = subCategoryId,
                    CategoryId = categoryId,
                    SubCategory1 = sc.Name,
                    Category = sc.DowntimeCategory.Name,
                    Inactive = sc.Inactive
                })
                .ToListAsync();
        }
    }
}