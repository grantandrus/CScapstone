using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.ViewModels
{
    public class ARCCViewModel
    {
        public List<User> Members { get; set; } = new();
        public List<User> OtherUsers { get; set; } = new();
        public int? CurrentChairId { get; set; }
        public List<Grant> SubmittedGrants { get; set; } = new(); // Final grants submitted for review
    }
}