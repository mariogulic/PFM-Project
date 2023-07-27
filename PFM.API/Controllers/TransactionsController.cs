using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Repositories;
using PFM.API.Utilities;
using System.Data;
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
        private readonly IRuleRepository _ruleRepository;
        private readonly IConfiguration _configuration;
        const int maxTransactionsPageSize = 20;

        public TransactionsController(ITransactionRepository transactionRepository, 
            IMapper mapper,
            ICategoryRepository categoryRepository,
            IRuleRepository ruleRepository,
            IConfiguration configuration)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _ruleRepository = ruleRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagedResponseModel<TransactionDto>>>> GetTransactions(DateTime? startDate, DateTime? endDate,
                                                                                       string? kinds, SortByEnum? sortBy, OrderByEnum? orderBy,
                                                                                       double? fromAmount, double? toAmount,
                                                                                      int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxTransactionsPageSize)
            {
                pageSize = maxTransactionsPageSize;
            }

            var allKindsAreValid = Helper.ValidateKinds(kinds);
            if (allKindsAreValid == false)
            {
                return NotFound(new
                {
                    Description = "Invalid kinds",
                    Message = "Please choose from the given kinds: 'dep', 'fee', 'pmt', 'sal', 'wdw'.",
                    StatusCode = 404
                });
            }

            if (fromAmount <= 0)
            {
                return BadRequest(new
                {
                    Desription = "You can't use negative numbers, please enter valid number"

                });
            }

            var (transactions, paginationMetaData) = await _transactionRepository.GetAllTransactionsAsync(startDate, endDate, kinds, sortBy, orderBy, pageNumber, pageSize, fromAmount, toAmount);


            var pagedResponse = new PagedResponseModel<TransactionDto>
            {
                PageSize = pageSize,
                Page = pageNumber,
                TotalCount = paginationMetaData.TotalItemCount,
                Items = _mapper.Map<IEnumerable<TransactionDto>>(transactions)
            };

            Response.Headers.Add("Pagination",
                JsonSerializer.Serialize(paginationMetaData));
            return Ok(pagedResponse);
        }

        [HttpPost("import")]
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
            var records = csv.GetRecords<TransactionDto>().ToList();

            var transactions = new List<Transaction>();
            foreach (var record in records)
            {
                var existingTransaction = await _transactionRepository.GetTransactionById(record.Id);
                if (existingTransaction == null)
                {
                    var transactionForDatase = new Transaction
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
            return Ok(new
            {
                Message = "Import successfully uploaded"
            });
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

            return Ok(new
            {
                Message = "Transaction successfully categorized."
            });
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
            if (totalAmount != transaction.Amount)
            {
                return StatusCode(400, new
                {
                    Description = "Total amount error",
                    Message = $"Total amount of splits is not equal to total amount of transaction which is {transaction.Amount}",
                    StatusCode = 404
                });
            }

            transaction.SplitTransactions.Clear();
            foreach (var splitTransactionItem in splitTransactionDto.Splits)
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

            return Ok(new
            {
                Message = "Transaction successfully splitted."
            });
        }

        [HttpPost("auto-categorize")]
        public async Task<ActionResult> AutoCategorize()
        {
            var rules = _configuration.GetSection("AutoCategorizeRules").Get<List<AutoCategorizeRule>>();

            if(rules == null || rules.Count == 0)
            {
                var databseRules = await _ruleRepository.GetAll(null);
                rules = _mapper.Map<List<AutoCategorizeRule>>(databseRules);

                if(rules.Count == 0)
                {
                    return StatusCode(400, new
                    {
                        Description = "No rules found",
                        Message = $"There arent any rules in config or in database",
                        StatusCode = 400
                    });
                }

            }

            var totalChanged = 0;
            foreach (var rule in rules)
            {
                var category = await _categoryRepository.GetCategoryBycode(rule.CatCode);
                if (category == null)
                {
                    return StatusCode(400, new
                    {
                        Description = "Error while getting category",
                        Message = "Category does not exist",
                        StatusCode = 400
                    });
                }

                try
                {
                    var affectedRows = await _transactionRepository.AutoCategorize(rule);
                    totalChanged += affectedRows;
                }
                catch (Exception ex)
                {
                    return StatusCode(400, new
                    {
                        Description = "Error while executing auto categorization query",
                        Message = $"Incorect predicate: '{rule.Predicate}'.",
                        StatusCode = 400
                    });
                }

            }

            return Ok(new
            {
                Message = $"Total {totalChanged} transaction successfully autocategorized."
            });
        }
    }
}


