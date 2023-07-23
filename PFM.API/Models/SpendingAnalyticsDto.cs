namespace PFM.API.Models
{
    public class SpendingAnalyticsDto
    {
        public List<SpendingAnalyticItemsDto> Groups { get; set; }
    }

    public class SpendingAnalyticItemsDto
    {
        public string CatCode { get; set; }
        public double Amount { get; set; }
        public int Count { get; set; }
    }
}
