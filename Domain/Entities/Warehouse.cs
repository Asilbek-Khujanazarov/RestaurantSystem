using System.Text.Json.Serialization;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    [JsonIgnore]
    public double OldQuantity { get; set; }
}
