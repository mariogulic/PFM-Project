using System.ComponentModel.DataAnnotations;

namespace PFM.API.Entities
{
    public class Rule
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string CatCode { get; set; }
        public string Predicate { get; set; }
    }
}
