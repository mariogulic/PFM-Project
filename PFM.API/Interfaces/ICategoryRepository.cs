using PFM.API.Entities;
using PFM.API.Models;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ICategoryRepository
    {
        Task AddCategory(Category categoryForDatabase);
        Task<Category> GetCategoryBycode(string code);
        Task AddCategories(List<Category> categories);
        Task UpdateCategory(Category existingCategory);
        Task<(IEnumerable<Category>, PaginationMetadata)> GetAll(string parentId, int pageNumber , int pageSize );
        Task AddCategoriesInBatch(List<CategoryDto> categories, int batchSize);

    }
}
