namespace RestaurantManagementSystem.Domain.Entities
{
    public class OrderRequest
    {
        public string OrderName { get; set; } 
        public List<OrderItemRequest> Items { get; set; }  
    }

    public class UpdateOrderRequestDto
    {
        public string OrderName { get; set; } = string.Empty;
        public List<OrderItemRequestDto> Items { get; set; } = new List<OrderItemRequestDto>();
    }

    public class OrderItemRequestDto
    {
        public string MenuName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}


