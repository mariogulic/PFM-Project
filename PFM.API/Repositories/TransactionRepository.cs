using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Repositories;



namespace PFM.API.TransactionRepository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<(IEnumerable<Transactions>, PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kind, int pageNumber, int pageSize)
        {
            var collection = _context.Transactions as IQueryable<Transactions>;

            if (!string.IsNullOrWhiteSpace(kind))
            {
                var validKinds = new List<string> { "dep", "fee", "pmt", "sal", "wdw" };

                if (validKinds.Contains(kind.ToLower()))
                {
                    collection = collection.Where(c => c.Kind == kind);
                }
            }


            if (startDate.HasValue || endDate.HasValue)
            {
                if (startDate.HasValue && endDate.HasValue)
                {
                    collection = collection.Where(t => t.Date >= startDate && t.Date < endDate);
                }
                else if (startDate.HasValue)
                {
                    collection = collection.Where(t => t.Date >= startDate);
                }
                else if (endDate.HasValue)
                {
                    collection = collection.Where(t => t.Date < endDate);
                }
            }

            var totalItemsCount = await collection.CountAsync();
            var paginationMetData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await collection
                .OrderBy(t => t.Id)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetData);
        }
        public async Task AddTransaction(Transactions transactionForDatase)
        {
            _context.Add(transactionForDatase);
            await _context.SaveChangesAsync();
        }

        public async Task<Transactions> GetTransactionById(int id)
        {
            return await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddTransactions(List<Transactions> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

    }
}

