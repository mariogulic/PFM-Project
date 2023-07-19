using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Repositories;

namespace PFM.API.Services
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        async Task<(IEnumerable<Transactions>, PaginationMetadata)> ITransactionRepository.GetAllTransactionsAsync(int pageNumber, int pageSize)
        {

            var totalItemsCount = await _context.Transactions.CountAsync();

            var paginationMetData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await _context.Transactions.OrderBy(t => t.Id)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetData);
        }

        public async Task<IEnumerable<Transactions>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions.Where(t => t.Date >= startDate && t.Date < endDate)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transactions>> GetTransactionsByKindAsync(string? kind)
        {

            if (string.IsNullOrEmpty(kind))
            {

                return await _context.Transactions.ToListAsync();
            }


            return await _context.Transactions.Where(t => t.Kind == kind).ToListAsync();
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
