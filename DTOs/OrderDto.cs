public class CreateOrderRequest
{
    public string OrderName { get; set; }
    public List<OrderItemRequest> Items { get; set; }
}

public class OrderItemRequest
{
    public string MenuName { get; set; }
    public int Quantity { get; set; }
    public string SizeKey { get; set; }
}

