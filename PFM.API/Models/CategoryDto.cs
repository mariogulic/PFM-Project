using CsvHelper.Configuration.Attributes;

namespace PFM.API.Models
{
    public class CategoryDto
    {
        [Name("code")]
        public string Code { get; set; } = string.Empty;
        [Name("parent-code")]
        public string ParentCode { get; set; } = string.Empty;
        [Name("name")]
        public string Name { get; set; } = string.Empty;
    }
}
