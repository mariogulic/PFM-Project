using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
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

            if(!string.IsNullOrWhiteSpace(kind))
            {
                kind = kind.Trim();
                collection = collection.Where(c =>  c.Kind == kind);
            }
            if(startDate != null || endDate != null)
            {
               collection = (IQueryable<Transactions>)collection.Where(t => t.Date >= startDate && t.Date < endDate)
                .OrderBy(t => t.Id)
                .ToListAsync();
            }
            var totalItemsCount = await  collection.CountAsync();

            var paginationMetData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await _context.Transactions.OrderBy(t => t.Id)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetData);
        }

        public async Task AddTransaction(Transactions transactionForDatase)
        {
            await _context.Transactions.AddAsync(transactionForDatase);
            await _context.SaveChangesAsync();
        }

        public async Task<Transactions> GetTransactionById(int id)
        {
            return await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);
        }
        
    }
}

