using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.ViewModels
{
    public class AllocationsViewModel
    {
        public List<Grant> AcceptedGrants { get; set; } = new();
        public List<Grant> RejectedGrants { get; set; } = new();
        public List<Grant> UndecidedGrants { get; set; } = new();
        public List<Grant> AllocatedGrants { get; set; } = new();
        public Allocation Allocation { get; set; } = new();
        public List<AllocationRule> Rules { get; set; } = new();
        public AllocationRule NewRule { get; set; } = new();
    }
}
