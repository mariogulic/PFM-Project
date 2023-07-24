using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFM.API.Entities
{
    public class SplitTransaction
    {
        [Key]
        public int Id { get; set; }

        public double Amount { get; set; }

        [ForeignKey("Category")]
        public string CatCode { get; set; }
        public Category Category { get; set; }

        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
    }
}
