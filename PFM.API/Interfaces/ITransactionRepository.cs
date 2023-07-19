using PFM.API.Entities;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transactions> , PaginationMetadata)> GetAllTransactionsAsync(DateTime? startDate, DateTime? endDate, string? kind,int pageNumber , int pageSize);
        Task AddTransaction(Transactions transactionForDatase);
        Task<Transactions> GetTransactionById(int id);
    }
}
