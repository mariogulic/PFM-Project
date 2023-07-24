using AutoMapper;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using PFM.API.Entities;
using PFM.API.Interfaces;
using PFM.API.Models;
using System.Globalization;

namespace PFM.API.Controllers
{
    [ApiController]
    [Route("api/categories")]

    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
 
        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        [HttpPost("importcategories")]
        public async Task<IActionResult> ImportFileCategory(IFormFile file)
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

            var categories = new List<Categories>();
            foreach (var record in records)
            {
                var existingCategory = await _categoryRepository.GetCategoryBycode(record.Code);
                if (existingCategory == null)
                {
                    var categoryForDatabase = new Categories
                    {
                        Code = record.Code,
                        ParentCode = record.ParentCode,
                        Name = record.Name

                    };
                    categories.Add(categoryForDatabase);
                }
                else
                {
                    existingCategory.Name = record.Name;
                    await _categoryRepository.UpdateCategory(existingCategory);
                }
              
            }
            await _categoryRepository.AddCategories(categories);
            return Ok("Import successfully uploaded");
        }
    }
}
