namespace PFM.API.Models
{
    public class SplitTransactionDto
    {
        public List<SplitTransactionItemDto> Splits { get; set; }
    }

    public class SplitTransactionItemDto
    {
        public string CatCode { get; set; }
        public double Amount { get; set; }
    }
}
