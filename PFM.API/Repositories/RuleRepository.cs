using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;

namespace PFM.API.Repositories
{
    public class RuleRepository : IRuleRepository
    {
        private readonly ApplicationDbContext _context;

        public RuleRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task AddRule(Rule rule)
        {
            _context.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Rule rule)
        {
            _context.Remove(rule);
            await _context.SaveChangesAsync();

        }

        public async Task<Rule> GetByid(int id)
        {
            return await _context.Rules.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Rule>> GetAll(string catCode)
        {
            var collection = _context.Rules as IQueryable<Rule>;

            if (!string.IsNullOrEmpty(catCode))
            {
                collection = collection.Where(x => x.CatCode == catCode);
            }

            return await collection.ToListAsync();
        }



        public async Task UpdateRule(Rule rule)
        {
            _context.Update(rule);
            await _context.SaveChangesAsync();
        }
    }
}
