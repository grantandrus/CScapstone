using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description {  get; set; }
        public List<Grant> Grants { get; set; } = new();
        // Nullable as Departments do not have a Chair when first created
        public int? ChairId { get; set; }
        public User? Chair { get; set; }
        public int? CollegeId { get; set; }
        public College? College { get; set; }
    }
}
