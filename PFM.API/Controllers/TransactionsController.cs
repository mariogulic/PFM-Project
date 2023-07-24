using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Utilities;
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
        public async Task<ActionResult<IEnumerable<PagedResponseModel<TransactionsDto>>>> GetTransactions(DateTime? startDate, DateTime? endDate, string? kinds, string? sortBy, OrderBy? orderBy,
                                                                                      int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxTransactionsPageSize)
            {
                pageSize = maxTransactionsPageSize;
            }

            var allKindsAreValid = Helper.ValidateKinds(kinds);
            if(allKindsAreValid == false)
            {
                return NotFound(new
                {
                    Description = "Invalid kinds",
                    Message = "Please choose from the given kinds: 'dep', 'fee', 'pmt', 'sal', 'wdw'.",
                    StatusCode = 404
                });
            }

            var (transactions, paginationMetaData) = await _transactionRepository.GetAllTransactionsAsync(startDate, endDate, kinds, sortBy, orderBy, pageNumber, pageSize);


            var pagedResponse = new PagedResponseModel<TransactionsDto>
            {
                PageSize = pageSize,
                Page = pageNumber,
                TotalCount = paginationMetaData.TotalItemCount,
                Items = _mapper.Map<IEnumerable<TransactionsDto>>(transactions)
            };

            Response.Headers.Add("Pagination",
                JsonSerializer.Serialize(paginationMetaData));
            return Ok(pagedResponse);
        }

        [HttpPost("importtransactions")]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return StatusCode(400, new
                {
                    Desctription = "Error while uploading file",
                    Message = "Please enter valid file",
                    StatusCode = 400,
                });

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
                return StatusCode(400, new
                {
                    Description = "Error while getting transaction",
                    Message = "Transaction does not exist",
                    StatusCode = 400
                });
            }

            var category = await _categoryRepository.GetCategoryBycode(categorizeTransactionDto.CatCode);

            if (category == null)
            {
                return StatusCode(400, new
                {
                    Description = "Error while getting category",
                    Message = "Category does not exist",
                    StatusCode = 400
                });
            }

            transaction.CatCode = category.Code;
            transaction.Category = category;

            await _transactionRepository.Update(transaction);

            return Ok("Transaction successfully categorized.");
        }


        [HttpPost("{id}/split")]
        public async Task<IActionResult> SplitTransaction(int id, SplitTransactionDto splitTransactionDto)
        {
            var transaction = await _transactionRepository.GetTransactionById(id);

            if (transaction == null)
            {
                return StatusCode(404, new
                {
                    Description = "Error while getting transaction",
                    Message = "Transaction does not exist",
                    StatusCode = 404
                });
            }

            var totalAmount = splitTransactionDto.Splits.Sum(x => x.Amount);
            if(totalAmount != transaction.Amount)
            {
                return StatusCode(400, new
                {
                    Description = "Total amount error",
                    Message = $"Total amount of splits is not equal to total amount of transaction which is {transaction.Amount}",
                    StatusCode = 404
                });
            }
            
            transaction.SplitTransactions.Clear();
            foreach(var splitTransactionItem in splitTransactionDto.Splits)
            {
                var category = await _categoryRepository.GetCategoryBycode(splitTransactionItem.CatCode);
                if (category == null)
                {
                    return StatusCode(404, new
                    {
                        Description = "Error while getting category",
                        Message = $"Category {splitTransactionItem.CatCode} does not exist",
                        StatusCode = 404
                    });
                }
               
                var splitTransaction = new SplitTransaction
                {
                    Amount = splitTransactionItem.Amount,
                    CatCode = category.Code,
                    TransactionId = transaction.Id
                };

                transaction.SplitTransactions.Add(splitTransaction);
            }

            await _transactionRepository.Update(transaction);

            return Ok("Transaction successfully splitted.");
        }
    }
}


