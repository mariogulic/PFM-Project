using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Interfaces;
using PFM.API.Models;

namespace PFM.API.Controllers
{

    [ApiController]
    [Route("api/spending-analytics")]
    public class SpendingAnalyticsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;


        public SpendingAnalyticsController(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SpendingAnalyticsDto>> GetSpendingAnalytics(string? catcode)
        {
            var spendingAnalyticItems = await _transactionRepository.GetSpendingAnalytics(catcode);

            var response = new SpendingAnalyticsDto
            {
                Groups = _mapper.Map<List<SpendingAnalyticItemsDto>>(spendingAnalyticItems)
            };

            return Ok(response);
        }
    }
}
