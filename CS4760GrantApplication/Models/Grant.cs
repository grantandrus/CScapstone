using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Grant
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string title { get; set; }
        [Required]
        [StringLength(300)]
        public string description { get; set; }
        [StringLength(400)]
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
