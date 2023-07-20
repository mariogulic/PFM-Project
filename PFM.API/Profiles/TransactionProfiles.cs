using AutoMapper;

namespace PFM.API.Profiles
{
    public class TransactionProfiles : Profile
    {
        public TransactionProfiles()
        {
            CreateMap<Entities.Transactions , Models.TransactionsDto>();
            CreateMap<Entities.Categories, Models.CategoryDto>();
        }
    }
}
