using PFM.API.Entities;
namespace PFM.API.Services
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transactions>> GetAllTransactionsAsync();
        Task<IEnumerable<Transactions>> GetTransactionsByDateAsync(DateTime startDate, DateTime endDate);

    }
}
