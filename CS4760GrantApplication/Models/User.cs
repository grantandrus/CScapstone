using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public List<Grant> Grants { get; set; } = new();
        public bool IsCommitteeMember { get; set; } = false;
        public bool IsCommitteeChair { get; set; } = false;
        public int? CollegeId { get; set; }
        public College? College { get; set; }
        public bool IsCollegeDean { get; set; } = false;
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public bool IsDepartmentChair { get; set; } = false;
    }
}
