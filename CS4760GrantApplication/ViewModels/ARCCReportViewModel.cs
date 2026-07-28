namespace CS4760GrantApplication.ViewModels
{
    public class ARCCReportViewModel
    {
        public List<SimpleReportViewModel> CollegeData { get; set; } = new();
        public List<SimpleReportViewModel> DepartmentData { get; set; } = new();
        public List<SimpleReportViewModel> RejectData { get; set; } = new();
        public List<SimpleReportViewModel> TopData { get; set; } = new();
        public decimal TotalAllocated { get; set; }
        public string? AllocatedString { get; set; }
    }
}
