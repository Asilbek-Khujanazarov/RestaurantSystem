using System.Text.Json.Serialization;

namespace RestaurantManagementSystem.Domain.Entities
{
    public class Categorie
    {
        public int Id { get; set; }
        public string CategorieName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        [JsonIgnore]
        public ICollection<Menu> Menus { get; set; } = new List<Menu>();  
        // Rasmlar ro'yxati
    }
}