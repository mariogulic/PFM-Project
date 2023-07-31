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

        public async Task<(IEnumerable<Transaction>, PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate,
            string? kinds, SortByEnum? sortBy,
            OrderByEnum? orderBy, int pageNumber,
            int pageSize, double? fromAmount, double? toAmount)
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



            switch (sortBy)
            {
                case SortByEnum.BeneficairyName:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.BeneficairyName) : collection.OrderBy(t => t.BeneficairyName);
                    break;
                case SortByEnum.Date:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Date) : collection.OrderBy(t => t.Date);
                    break;
                case SortByEnum.Direction:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Direction) : collection.OrderBy(t => t.Direction);
                    break;
                case SortByEnum.Amount:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Amount) : collection.OrderBy(t => t.Amount);
                    break;
                case SortByEnum.Description:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Description) : collection.OrderBy(t => t.Description);
                    break;
                case SortByEnum.Currency:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Currency) : collection.OrderBy(t => t.Currency);
                    break;
                case SortByEnum.Mcc:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Mcc) : collection.OrderBy(t => t.Mcc);
                    break;
                case SortByEnum.Kind:
                    collection = orderBy == OrderByEnum.Desc ? collection.OrderByDescending(t => t.Kind) : collection.OrderBy(t => t.Kind);
                    break;
                default: 
                    collection = collection.OrderBy(t => t.Date);
                    break;
            }


            if (fromAmount.HasValue && toAmount.HasValue)
            {
                collection = collection.Where(t => t.Amount >= fromAmount && t.Amount < toAmount);
            }
            else if (fromAmount.HasValue)
            {
                collection = collection.Where(t => t.Amount >= fromAmount);
            }
            else if (toAmount.HasValue)
            {
                collection = collection.Where(t => t.Amount < toAmount);
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

        public async Task<int> AutoCategorize(AutoCategorizeRule rule)
        {
            var rowsModified = await _context.Database.ExecuteSqlRawAsync(@$"UPDATE Transactions 
                                                                    SET CatCode = '{rule.CatCode}' 
                                                                    WHERE CatCode is NULL AND {rule.Predicate}");
            return rowsModified;
        }

        public async Task AddTransactionsInBatch(List<TransactionDto> records, int batchSize)
        {
            var transactions = new List<Transaction>();
            
            var counter = 0;
            foreach (var record  in records) 
            {
                var transactionForDatase = new Transaction
                {
                    Id = record.Id,
                    BeneficairyName = record.BeneficairyName,
                    Date = record.Date,
                    Direction = record.Direction,
                    Amount = record.Amount,
                    Description = record.Description,
                    Currency = record.Currency,
                    Mcc = record.Mcc,
                    Kind = record.Kind,
                };
                transactions.Add(transactionForDatase);
                counter++;

                if(counter == batchSize)
                {
                    var transactionIds = transactions.Select(x => x.Id).ToList();
                    var existingIds = _context.Transactions.Where(x => transactionIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    transactions = transactions.Where(x => !existingIds.Contains(x.Id)).ToList();   


                    await _context.Transactions.AddRangeAsync(transactions);
                    await _context.SaveChangesAsync();

                    transactions = new List<Transaction>();
                    counter = 0;
                }
            }

            if(counter > 0)
            {
                var transactionIds = transactions.Select(x => x.Id).ToList();
                var existingIds = _context.Transactions.Where(x => transactionIds.Contains(x.Id)).Select(x => x.Id).ToList();
                transactions = transactions.Where(x => !existingIds.Contains(x.Id)).ToList();

                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();
            }
            return;
        }
    }
}

