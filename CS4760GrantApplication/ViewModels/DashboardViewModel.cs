using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.ViewModels
{
    public class DashboardViewModel
    {
        public List<Grant> MyGrants { get; set; } = new();
        public List<Grant> DepartmentGrants { get; set; } = new();
        public List<Grant> CollegeGrants { get; set; } = new();
        public bool IsDeptChair { get; set; }
        public bool IsCollegeDean { get; set; }
    }
}