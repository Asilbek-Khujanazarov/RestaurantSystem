
public class Order
{
    public int Id { get; set; }
    public int CustomId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public bool? Process { get; set; } = null;

    public List<OrderItem> OrderItems { get; set; }
}