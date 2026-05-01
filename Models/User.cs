using System.ComponentModel.DataAnnotations;
namespace BusBookingSystem.Models
{
    public class User{
    public int Id {get; set;}
    [Required]//ensures field can't be empty
    public string Username {get; set;}=string.Empty;
    [Required]
    [EmailAddress]//ensures the value is a proper email format
    public string Email {get; set;}=string.Empty;
    // input only (NOT stored in DB)
    public string Password { get; set; } = string.Empty;
    [Required]
    public string PasswordHash {get; set;}=string.Empty;
    }



}