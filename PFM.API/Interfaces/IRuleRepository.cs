using PFM.API.Entities;

namespace PFM.API.Interfaces
{
    public interface IRuleRepository
    {
        Task<Rule> GetByid(int id);
        Task<IEnumerable<Rule>> GetAll(string catCode);
        Task AddRule(Rule rule);
        Task UpdateRule(Rule rule);
        Task Delete(Rule rule);
    }
}
