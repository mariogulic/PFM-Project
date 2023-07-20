using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using System.Globalization;
using System.Text.Json;


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
        public async Task<ActionResult<IEnumerable<TransactionsDto>>> GetTransactions(DateTime? startDate, DateTime? endDate, string? kind, int pageNumber = 1, int pageSize = 10)
        {

            if (pageSize > maxTransactionsPageSize)
            {
                pageSize = maxTransactionsPageSize;
            }

            try
            {
                var (transactions, paginationMetaData) = await _transactionRepository.GetAllTransactionsAsync(startDate, endDate, kind, pageNumber, pageSize);

                Response.Headers.Add("Pagination",
                    JsonSerializer.Serialize(paginationMetaData));

                return Ok(_mapper.Map<IEnumerable<TransactionsDto>>(transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("importtransactions")]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("The File is not valid");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<TransactionsDto>().ToList();

            var transactions = new List<Transactions>();
            foreach (var record in records)
            {
                var existingTransaction = await _transactionRepository.GetTransactionById(record.Id);
                if (existingTransaction == null)
                {
                    var transactionForDatase = new Transactions
                    {
                        Id = record.Id,
                        BeneficairyName = record.BeneficairyName,
                        Date = record.Date,
                        Direction = record.Direction,
                        Amount = record.Amount,
                        Description = record.Description,
                        Currency = record.Currency,
                        Mcc = record.Mcc,
                        Kind = record.Kind
                    };
                    transactions.Add(transactionForDatase);
                }
            }
            await _transactionRepository.AddTransactions(transactions);
            return Ok();
        }
    }
}