using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class College
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Nullable as Colleges do not have a Dean when first created
        public int? DeanId { get; set; }
        public User? Dean { get; set; }

        public List<Grant> Grants { get; set; } = new();
    }
}
