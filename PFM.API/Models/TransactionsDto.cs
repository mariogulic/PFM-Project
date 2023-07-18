namespace PFM.API.Models
{
    public class TransactionsDto
    {
        public int Id { get; set; }
        public string BeneficairyName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public char Direction { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int Mcc { get; set; }
        public string Kind { get; set; } = string.Empty;
    }
}
