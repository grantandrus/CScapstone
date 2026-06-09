using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class RatingSuggestion
    {
        public int Id { get; set; }
        [Required]
        public int Score { get; set; }
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}
