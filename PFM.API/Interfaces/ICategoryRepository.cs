using PFM.API.Entities;

namespace PFM.API.Interfaces
{
    public interface ICategoryRepository
    {
        Task AddCategory(Category categoryForDatabase);
        Task<Category> GetCategoryBycode(string code);
        Task AddCategories(List<Category> categories);
        Task UpdateCategory(Category existingCategory);
        Task<IEnumerable<Category>> GetAll(string parentId);

    }
}
