using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PFM.API.DbContexts;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Repositories;
using PFM.API.Utilities;

namespace PFM.API.TransactionRepository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<(IEnumerable<Transaction>, PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kinds, string? sortBy, OrderByEnum? orderBy, int pageNumber, int pageSize)
        {

            var collection = _context.Transactions.Include(x => x.SplitTransactions) as IQueryable<Transaction>;

            if (!string.IsNullOrWhiteSpace(kinds))
            {
                var inputKinds = kinds?.Split(',').Select(k => k.Trim().ToLower()) ?? new List<string>();
                collection = collection.Where(c => inputKinds.Contains(c.Kind.ToLower()));
            }


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


            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "id":
                        collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Id) : collection.OrderBy(t => t.Id);
                        break;
                    case "beneficiaryname":
                        collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.BeneficairyName) : collection.OrderBy(t => t.BeneficairyName);
                        break;
                    case "date":
                        collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Date) : collection.OrderBy(t => t.Date);
                        break;
                    case "amount":
                        collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Amount) : collection.OrderBy(t => t.Amount);
                        break;
                    default:
                        throw new ArgumentException($"Please choose from the given cases");
                }
            }
            else
            {
                collection = collection.OrderBy(t => t.Date);
            }

            var totalItemsCount = await collection.CountAsync();
            var paginationMetData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await collection
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetData);
        }
        public async Task AddTransaction(Transaction transactionForDatase)
        {
            _context.Add(transactionForDatase);
            await _context.SaveChangesAsync();
        }

        public async Task<Transaction> GetTransactionById(int id)
        {
            return await _context.Transactions
                .Include(x => x.SplitTransactions)
                    .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddTransactions(List<Transaction> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SpendingAnalyticItem>> GetSpendingAnalytics(string catcode, DateTime? endDate, DateTime? startDate, DirectionEnum? direction)
        {
            var collection = _context.Transactions.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                collection = collection.Where(t => t.Date >= startDate.Value && t.Date < endDate.Value);
            }
            else if (startDate.HasValue)
            {
                collection = collection.Where(t => t.Date >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                collection = collection.Where(t => t.Date < endDate.Value);
            }

            if (!string.IsNullOrEmpty(catcode))
            {
                collection = collection.Where(x => x.Category.Code == catcode || x.Category.ParentCode == catcode);
            }

            if (direction == DirectionEnum.D)
            {
                collection = collection.Where(x => x.Direction == "d");
            }
            else if (direction == DirectionEnum.C)
            {
                collection = collection.Where(x => x.Direction == "c");
            }

            var groupedTransactions = await collection
                .GroupBy(x => x.Category.Code)
                .Select(group => new SpendingAnalyticItem
                {
                    CatCode = group.Key,
                    Amount = group.Sum(x => x.Amount),
                    Count = group.Count()
                })
                .ToListAsync();

            return groupedTransactions;
        }

        public async Task<bool> CheckCatcodeExistsAsync(string catcode)
        {
            return await _context.Categories.AnyAsync(c => c.Code == catcode);
        }
    }
}

