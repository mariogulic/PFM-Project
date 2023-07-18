using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Models;
using PFM.API.Services;

namespace PFM.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionsController(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository 
                ?? throw new ArgumentNullException(nameof(transactionRepository));

            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionsDto>>> GetTransactions()
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync();

            return Ok(_mapper.Map<IEnumerable<TransactionsDto>>(transactions));
        }

        [HttpGet("transactionsbydate")]
        public async Task<ActionResult<IEnumerable<TransactionsDto>>> GetTransactionsByDate(DateTime startDate, DateTime endDate)
        {
            var transactionsByDate = await _transactionRepository.GetTransactionsByDateAsync(startDate, endDate);

            return Ok(_mapper.Map<IEnumerable<TransactionsDto>>(transactionsByDate));
        }

    }
}

