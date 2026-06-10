using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.ViewModels
{
    public class DepartmentCreateViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string DepartmentName { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        public int? ChairId { get; set; }
        public int? CollegeId { get; set; }
    }
}
