public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CustomId { get; set; }
    public string MenuName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalItemPrice => Quantity * Price;
}