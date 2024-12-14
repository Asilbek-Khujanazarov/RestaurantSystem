public class UpdateOrderRequestDto
{
    public string OrderName { get; set; }
    public List<UpdateOrderItemRequestDto> Items { get; set; }
}

public class UpdateOrderItemRequestDto
{
    public string MenuName { get; set; }
    public int Quantity { get; set; }
    public string SizeKey { get; set; } 
}


