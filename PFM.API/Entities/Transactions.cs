using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFM.API.Entities
{
    public class Transactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string BeneficairyName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string Direction { get; set; }
        public double Amount { get; set; }
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; }
        public string Mcc { get; set; }
        public string Kind { get; set; }

        public Transactions()
        {
            
        }

        public Transactions(string beneficairyName) 
        {
            BeneficairyName = beneficairyName;
        }
    }
}
