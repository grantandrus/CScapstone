using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public decimal AverageScore { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
        public int GrantId { get; set; }
        public Grant? Grant { get; set; }
    }
}
