using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Services;
using System.Text.Json;
using System.Transactions;

namespace PFM.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        const int maxTransactionsPageSize = 20;

        public TransactionsController(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionsDto>>> GetTransactions(int pageNumber = 1, int pageSize = 10)
        {

            if (pageSize > maxTransactionsPageSize)
            {
                pageNumber = maxTransactionsPageSize;
            }
            var (transactions, paginationMetaData) = await _transactionRepository.GetAllTransactionsAsync(pageNumber, pageSize);

            Response.Headers.Add("Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            return Ok(_mapper.Map<IEnumerable<TransactionsDto>>(transactions));
        }

        [HttpGet("transactionsbydate")]
        public async Task<ActionResult<IEnumerable<TransactionsDto>>> GetTransactionsByDate(DateTime startDate, DateTime endDate)
        {
            var transactionsByDate = await _transactionRepository.GetTransactionsByDateAsync(startDate, endDate);

            return Ok(_mapper.Map<IEnumerable<TransactionsDto>>(transactionsByDate));
        }

        [HttpGet("kind")]
        public async Task<IActionResult> GetTransactionsByKind(string? kind)
        {
            var filtered = await _transactionRepository.GetTransactionsByKindAsync(kind);
            if (kind == null)
            {
                return Ok(_transactionRepository.GetAllTransactionsAsync(1, 10));
            }
            return Ok(filtered);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please enter valid File");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var csvString = await reader.ReadToEndAsync();
                var parts = csvString.Split("\n").ToList();
                parts = parts.Skip(1).ToList();

                foreach (var transactionString in parts)
                {
                    if (string.IsNullOrEmpty(transactionString))
                        continue;

                    var transactionsParts = transactionString.Split(",").ToList();

                    var id = int.Parse(transactionsParts[0]);
                    var existingTransaction = await _transactionRepository.GetTransactionById(id);
                    if (existingTransaction == null)
                    {
                        var transactionForDatase = new Transactions
                        {
                            Id = id,
                            BeneficairyName = transactionsParts[1],
                            Currency = "",
                            Amount = 0,
                            Date = DateTime.Now,
                            Description = "",
                            Direction = 'a',
                            Kind = "",
                            Mcc = 1
                        };

                        await _transactionRepository.AddTransaction(transactionForDatase);
                    }

                }

            }


            return Ok();

        }
    }
}

