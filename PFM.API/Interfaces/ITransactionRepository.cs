using PFM.API.Entities;
using PFM.API.Repositories;

namespace PFM.API.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transactions> , PaginationMetadata)> GetAllTransactionsAsync(int pageNumber , int pageSize);
        Task<IEnumerable<Transactions>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transactions>> GetTransactionsByKindAsync(string? kind);
        Task AddTransaction(Transactions transactionForDatase);
        Task<Transactions> GetTransactionById(int id);
    }
}
