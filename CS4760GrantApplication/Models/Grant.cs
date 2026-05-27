using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Grant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string title { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string description { get; set; } = string.Empty;

        public int? UserId { get; set; }
        public User? User { get; set; }

        public bool InvolvesHumanOrAnimalSubjects { get; set; }

        public List<GrantAttachment> Attachments { get; set; } = new();
    }
}