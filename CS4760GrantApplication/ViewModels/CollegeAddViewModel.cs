using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.ViewModels
{
    public class CollegeAddViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        public int? DeanId { get; set; }
    }
}
