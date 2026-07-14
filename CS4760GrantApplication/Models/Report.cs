using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int GrantId { get; set; }
        public Grant? Grant { get; set; }
        public string? ProjectSummary { get; set; } = string.Empty;
        public string? Budget { get; set; } = string.Empty;
        public string? CurrentProgress { get; set; } = string.Empty;
        public string? NextSteps { get; set; } = string.Empty;
    }
}
