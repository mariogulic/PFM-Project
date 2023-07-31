using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using PFM.API.Repositories;
using System.Globalization;
using System.Text.Json;

namespace PFM.API.Controllers
{
    [ApiController]
    [Route("api/categories")]

    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        const int maxTransactionsPageSize = 20;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponseModel<CategoryDto>>> GetAll([FromQuery(Name = "parent-id")] string? parentId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxTransactionsPageSize)
            {
                pageSize = maxTransactionsPageSize;
            }


            (IEnumerable<Category> categories, PaginationMetadata paginationMetaData) = await _categoryRepository.GetAll(parentId, pageNumber, pageSize);

            if (!categories.Any())
            {
                return NotFound(new 
                {
                    Message = "Parent ID does not exist."
                });
            }

            var pagedResponse = new PagedResponseModel<TransactionDto>
            {
                PageSize = pageSize,
                Page = pageNumber,
                TotalCount = paginationMetaData.TotalItemCount,
                Items = _mapper.Map<IEnumerable<TransactionDto>>(categories)
            };
            Response.Headers.Add("Pagination",
               JsonSerializer.Serialize(paginationMetaData));
            return Ok(pagedResponse);
        }


        [HttpPost("import")]
        public async Task<ActionResult> ImportFileCategory(IFormFile file)
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
            var records = csv.GetRecords<CategoryDto>().ToList();

            await _categoryRepository.AddCategoriesInBatch(records , 100);
            return Ok(new
            {
                Message = "Import successfully uploaded"
            });
        }
    }
}
