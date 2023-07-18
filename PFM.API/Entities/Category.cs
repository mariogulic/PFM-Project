using System.ComponentModel.DataAnnotations;

namespace PFM.API.Entities
{
    public class Category
    {
   
        public int Code { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
