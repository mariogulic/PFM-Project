using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFM.API.Entities
{
    public class Categories
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Code { get; set; } = string.Empty;

        public string ParentCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

 
        public Categories()
        {
            
        }
    }
}
