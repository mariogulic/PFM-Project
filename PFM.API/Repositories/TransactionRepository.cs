using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;


namespace PFM.API.Services
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Transactions>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<IEnumerable<Transactions>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions.Where(t => t.Date >= startDate && t.Date < endDate)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }
    }
}
