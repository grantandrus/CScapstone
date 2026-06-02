using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.ViewModels
{
    public class ARCCViewModel
    {
        public List<User> Members { get; set; }
        public List<User> OtherUsers { get; set; }
        public int? CurrentChairId { get; set; }
    }
}