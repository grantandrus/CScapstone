using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public bool IsAdmin { get; set; } = false;
    }
}
