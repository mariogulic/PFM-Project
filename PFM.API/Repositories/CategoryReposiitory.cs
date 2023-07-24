using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;

namespace PFM.API.Repositories
{
    public class CategoryReposiitory : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryReposiitory(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddCategory(Category categoryForDatase)
        {
            _context.Add(categoryForDatase);
            await _context.SaveChangesAsync();
        }
        public async Task<Category> GetCategoryBycode(string code)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task AddCategories(List<Category> categories)
        {
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Category existingCategory)
        {
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAll(string parentId)
        {
            var collection = _context.Categories as IQueryable<Category>;

            if (!string.IsNullOrEmpty(parentId))
            {
                collection = collection.Where(x => x.ParentCode == parentId);
            }

            return await collection.ToListAsync();
        }
    }
}
