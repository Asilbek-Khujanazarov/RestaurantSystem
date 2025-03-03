public class PaymentRequest
{
    public int CustomId { get; set; } // Mijozdan kelgan order ID
    public string PaymentMethodId { get; set; } // Stripe PaymentMethod ID (Token)
}
