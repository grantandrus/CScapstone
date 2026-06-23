using CS4760GrantApplication.Models; 

namespace CS4760GrantApplication.ViewModels
{
    public class BudgetWorksheetViewModel
    {
        public int GrantId { get; set; }
        public string GrantTitle { get; set; } = string.Empty;
        public List<BudgetItem> BudgetItems { get; set; } = new();

        public decimal TotalRequested => BudgetItems.Sum(b => b.Amount);

        public decimal TotalFromARCC => BudgetItems
            .Where(b => b.FundingSource == "ARCC")
            .Sum(b => b.Amount);

        public decimal TotalFromCollege => BudgetItems
            .Where(b => b.FundingSource == "College")
            .Sum(b => b.Amount);

        public decimal TotalFromDepartment => BudgetItems
            .Where(b => b.FundingSource == "Department")
            .Sum(b => b.Amount);

        public decimal TotalFromOther => BudgetItems
            .Where(b => b.FundingSource == "Other")
            .Sum(b => b.Amount);

    }
}
