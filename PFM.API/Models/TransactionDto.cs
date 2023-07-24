using CsvHelper.Configuration.Attributes;

namespace PFM.API.Models
{
    public class TransactionDto
    {
        [Name("id")]
        public int Id { get; set; }

        [Name("beneficiary-name")]
        public string BeneficairyName { get; set; } = string.Empty;

        [Name("date")]
        public DateTime Date { get; set; }

        [Name("direction")]
        public string Direction { get; set; } = string.Empty;

        [Name("amount")]
        public double Amount { get; set; }

        [Name("description")]
        public string Description { get; set; } = string.Empty;

        [Name("currency")]
        public string Currency { get; set; } = string.Empty;

        [Name("mcc")]
        public string Mcc { get; set; } = string.Empty;

        [Name("kind")]
        public string Kind { get; set; } = string.Empty;

        public List<SplitTransactionItemDto> SplitTransactions { get; set; } = new List<SplitTransactionItemDto>();
    }
}
