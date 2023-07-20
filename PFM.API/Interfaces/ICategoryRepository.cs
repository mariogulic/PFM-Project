using PFM.API.Entities;

namespace PFM.API.Interfaces
{
    public interface ICategoryRepository
    {
        Task AddCategory(Categories categoryForDatabase);
        Task<Categories> GetCategoryBycode(string code);
        Task AddCategories(List<Categories> categories);
      
        
    }
}
