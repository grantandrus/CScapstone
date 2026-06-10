using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CS4760GrantApplication.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public int GrantId { get; set; }
        public Grant? Grant { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ItemType { get; set; } = string.Empty; // Hardware or Software

        [Required]
        public string FundingSource { get; set; } = string.Empty; // ARCC, College, Department, Other

        [Required]
        [Range(0.01, 1000000)]
        [Precision(18, 2)]
        public decimal Amount { get; set; }
    }
}