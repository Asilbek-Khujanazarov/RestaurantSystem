using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required]
    public int PhoneNumber { get; set; }

    [Required]
    public string Password { get; set; }
}
