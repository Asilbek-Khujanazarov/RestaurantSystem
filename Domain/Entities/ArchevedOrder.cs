using RestaurantManagementSystem.Domain.Entities;
public class ArchevedOrder
{
    public int Id { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;

    public bool? Process { get; set; } = null;  
}