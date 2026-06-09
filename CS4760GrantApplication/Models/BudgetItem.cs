using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public int GrantId { get; set; }
        public Grant? Grant { get; set; }

        [Required]
        public string ItemType { get; set; } = string.Empty; // Hardware or Software

        [Required]
        public string FundingSource { get; set; } = string.Empty; // ARCC, College, Department, Other

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }
    }
}