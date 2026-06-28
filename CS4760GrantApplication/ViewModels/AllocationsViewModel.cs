using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.ViewModels
{
    public class AllocationsViewModel
    {
        public List<Grant> Grants { get; set; } = new();
        public Allocation Allocation { get; set; } = new();
    }
}
