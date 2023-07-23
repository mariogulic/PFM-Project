using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.TransactionRepository;

namespace PFM.API.Repositories
{
    public class CategoryReposiitory : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryReposiitory(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task AddCategory(Categories categoryForDatase)
        {
            _context.Add(categoryForDatase);
            await _context.SaveChangesAsync();
        }
        public async Task<Categories> GetCategoryBycode(string code)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.Code == code);
        }
        
        public async Task AddCategories(List<Categories> categories)
        {
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Categories existingCategory)
        {
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
        }
    }
}
