using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class AllocationRule
    {
        public int Id { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal MinScore { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal MaxScore { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal PercentAllocated { get; set; }
    }
}
