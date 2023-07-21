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
        private readonly ICategoryRepository _categoryRepository;
        const int maxTransactionsPageSize = 20;

        public TransactionsController(ITransactionRepository transactionRepository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagedResponseModel<TransactionsDto>>>> GetTransactions(DateTime? startDate, DateTime? endDate, string? kind, string? sortBy, string? orderBy,
                                                                                      int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxTransactionsPageSize)
            {
                pageSize = maxTransactionsPageSize;
            }

            try
            {
                var (transactions, paginationMetaData) = await _transactionRepository.GetAllTransactionsAsync(startDate, endDate, kind, sortBy, orderBy, pageNumber, pageSize);

                var pagedResponse = new PagedResponseModel<TransactionsDto>
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    TotalCount = paginationMetaData.TotalItemCount,
                    Items = _mapper.Map<IEnumerable<TransactionsDto>>(transactions)
                };

                Response.Headers.Add("Pagination",
                    JsonSerializer.Serialize(paginationMetaData));
                return Ok(pagedResponse);

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
                return BadRequest("File is not ok");
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
            return Ok("Import successfully uploaded");
        }


        [HttpPost("{id}/categorize")]
        public async Task<IActionResult> CategorizeTransaction(int id, CategorizeTransactionDto categorizeTransactionDto)
        {
            var transaction = await _transactionRepository.GetTransactionById(id);

            if (transaction == null)
            {
                return NotFound("Transaction not found.");
            }

            var category = await _categoryRepository.GetCategoryBycode(categorizeTransactionDto.CatCode);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            transaction.CatCode = category.Code;
            transaction.Category = category;

            await _transactionRepository.Update(transaction);

            return Ok("Transaction successfully categorized.");
        }
    }
}


