using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PFM.API.Entities;
using PFM.API.Models;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transaction>, PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kinds, SortByEnum? sortBy, OrderByEnum? orderBy, 
                                                                                     int pageNumber, int pageSize, double? fromAmount, double? toAmount);
        Task AddTransaction(Transaction transactionForDatase);
        Task<Transaction> GetTransactionById(int id);
        Task AddTransactions(List<Transaction> transactions);
        Task Update(Transaction transaction);
        Task<List<SpendingAnalyticItem>> GetSpendingAnalytics(string catcode, DateTime? startDate, DateTime? endDate, DirectionEnum? direction);
        Task<bool> CheckCatcodeExistsAsync(string catcode);
        Task<int> AutoCategorize(AutoCategorizeRule rule);
        Task AddTransactionsInBatch(List<TransactionDto> transactions , int batchSize);
    }
}
