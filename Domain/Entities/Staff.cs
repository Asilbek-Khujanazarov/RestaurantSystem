using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Staff
{
    [JsonIgnore] 
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string SurName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public int PhoneNumber { get; set; }  
    [Required]
    public string Password { get; set; }
    [JsonIgnore] 
    public int CustomId { get; set; }
    public bool? IsSuperStaff { get; set; } = null;
}