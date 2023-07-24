using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFM.API.Entities
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string BeneficairyName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Direction { get; set; } = string.Empty;
        public double Amount { get; set; }
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;

        public Category? Category { get; set; }
        public string? CatCode { get; set; }

        public ICollection<SplitTransaction> SplitTransactions { get; set; }

        public Transaction()
        {
            SplitTransactions = new List<SplitTransaction>();
        }
        public Transaction(string beneficairyName)
        {
            BeneficairyName = beneficairyName;
            SplitTransactions = new List<SplitTransaction>();
        }
    }
}
