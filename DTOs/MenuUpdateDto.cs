public class MenuUpdateDto
{
    public string Name { get; set; }
    public int QuantityProduct { get; set; }
    public string Description { get; set; }
    public List<KeyValuePair<string, decimal>> Sizes { get; set; }
}
