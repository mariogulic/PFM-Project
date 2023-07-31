using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;

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

        //public async Task AddCategories(List<Category> categories)
        //{
        //    await _context.Categories.AddRangeAsync(categories);
        //    await _context.SaveChangesAsync();
        //}

        public async Task UpdateCategory(Category existingCategory)
        {
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Category>, PaginationMetadata)> GetAll(string parentId, int pageNumber, int pageSize)
        {
            var collection = _context.Categories as IQueryable<Category>;

           
            if (!string.IsNullOrEmpty(parentId))
            {
                collection = collection.Where(x => x.ParentCode == parentId);
            }
            var totalItemsCount = await collection.CountAsync();
            var paginationMetData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await collection
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return  (collectionToReturn , paginationMetData);
        }

        public async Task AddCategoriesInBatch(List<CategoryDto> records, int batchSize)
        {
            var categories = new List<Category>();

            var counter = 0;
            foreach (var record in records)
            {
                var categoryForDatase = new Category
                {
                   Code = record.Code,
                   Name = record.Name,
                   ParentCode = record.ParentCode,
                };
                categories.Add(categoryForDatase);
                counter++;

                if (counter == batchSize)
                {
                    await _context.Categories.AddRangeAsync(categories);
                    await _context.SaveChangesAsync();

                    categories = new List<Category>();
                    counter = 0;
                }
            }
            if (counter > 0)
            {
                await _context.Categories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
            }
        }
    }
}
