using AutoMapper;

namespace PFM.API.Profiles
{
    public class TransactionProfiles : Profile
    {
        public TransactionProfiles()
        {
            CreateMap<Entities.Transaction , Models.TransactionDto>();
            CreateMap<Entities.Category, Models.CategoryDto>();
            CreateMap<Entities.SpendingAnalyticItem, Models.SpendingAnalyticItemsDto>();
            CreateMap<Entities.SplitTransaction, Models.SplitTransactionItemDto>();
        }
    }
}

