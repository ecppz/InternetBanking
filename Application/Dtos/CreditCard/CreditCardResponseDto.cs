namespace Application.Dtos.CreditCard
{
    public class CreditCardResponseDto
    {
        public bool Success { get; set; }       
        public string Message { get; set; }       
        public Guid? CardId { get; set; }       
        public string? CardNumber { get; set; }   
        public decimal? CreditLimit { get; set; } 
        public decimal? CurrentDebt { get; set; }  
        public DateTime? ExpirationDate { get; set; }
    }
}
