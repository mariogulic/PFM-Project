using Microsoft.Data.SqlClient;
using PFM.API.Entities;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transactions> , PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kind,string? sortBy, string? orderBy , int pageNumber , int pageSize);
        Task AddTransaction(Transactions transactionForDatase);
        Task<Transactions> GetTransactionById(int id);
        Task AddTransactions(List<Transactions> transactions);
        Task Update(Transactions transaction);
    }
}
