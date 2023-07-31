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
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<SpendingAnalyticsDto>> GetSpendingAnalytics(string? catcode, DateTime? startDate, DateTime? endDate, DirectionEnum? direction)
        {


            if (!string.IsNullOrEmpty(catcode))
            {
                bool catcodeExists = await _transactionRepository.CheckCatcodeExistsAsync(catcode);
                if (!catcodeExists)
                {
                    return StatusCode(404, new
                    {
                        Description = "Error while getting CatCode",
                        Message = "Invalid catcode. The specified catcode does not exist.",
                        StatusCode = 404
                    });
                }
            }


            var spendingAnalyticItems = await _transactionRepository.GetSpendingAnalytics(catcode, endDate, startDate, direction);
            
            var response = new SpendingAnalyticsDto
            {
                Groups = _mapper.Map<List<SpendingAnalyticItemsDto>>(spendingAnalyticItems)
            };

            return Ok(response);
        }
    }
}
