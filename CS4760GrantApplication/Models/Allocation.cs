using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Allocation
    {
        public int Id { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal AvailableAmount { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal RolloverAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal CutoffPercent { get; set; }
    }
}