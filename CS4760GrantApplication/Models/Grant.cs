using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Grant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string ProjectSummary { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Justification { get; set; } = string.Empty;

        [Required]
        public int ProjectImpact { get; set; }

        [Required]
        [StringLength(2000)]
        public string ProjectTimeline { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string SuccessEvaluation { get; set; } = string.Empty;

        public bool isMultipleDepartments { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public bool InvolvesHumanOrAnimalSubjects { get; set; }

        public List<GrantAttachment> Attachments { get; set; } = new();
    }
}