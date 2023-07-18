using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFM.API.Entities
{
    public class Transactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string BeneficairyName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public char Direction { get; set; }
        public double Amount { get; set; }
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; }
        public int Mcc { get; set; }
        public string Kind { get; set; }



        public Transactions(string beneficairyName) 
        {
            BeneficairyName = beneficairyName;
        }
    }
}
