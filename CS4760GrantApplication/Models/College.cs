using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class College
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // project requirements from Sprint 1 allow College to be created without a dean, so this is nullable
        public User? Dean { get; set; }
    }
}
