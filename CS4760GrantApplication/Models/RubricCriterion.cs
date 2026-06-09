using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class RubricCriterion
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int MaxScore { get; set; }
        public List<RatingSuggestion> RatingSuggestions { get; set; } = new();
    }
}
