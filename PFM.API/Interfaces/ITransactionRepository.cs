using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PFM.API.Entities;
using PFM.API.Models;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transactions>, PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kinds, string? sortBy, OrderBy? orderBy, int pageNumber, int pageSize);
        Task AddTransaction(Transactions transactionForDatase);
        Task<Transactions> GetTransactionById(int id);
        Task AddTransactions(List<Transactions> transactions);
        Task Update(Transactions transaction);
        Task<List<SpendingAnalyticItem>> GetSpendingAnalytics(string catcode, DateTime? startDate, DateTime? endDate, DirectionEnum? direction);
        Task<bool> CheckCatcodeExistsAsync(string catcode);

    }
}
