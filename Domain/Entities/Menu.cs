using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RestaurantManagementSystem.Domain.Entities
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPresent { get; set; } = true;
        [Required]
        public string Type { get; set; }
        public bool Considered { get; set; }
        public double? QuantityProduct { get; set; } = null;

        // JSON formatida saqlanadigan SizesJson
        public string SizesJson { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // [NotMapped] EF Core tomonidan xaritalanmaydi
        [NotMapped]
        public List<KeyValuePair<string, decimal>> Sizes
        {
            get => string.IsNullOrEmpty(SizesJson) ? new List<KeyValuePair<string, decimal>>() : JsonSerializer.Deserialize<List<KeyValuePair<string, decimal>>>(SizesJson);
            set => SizesJson = JsonSerializer.Serialize(value);
        }
    }
}