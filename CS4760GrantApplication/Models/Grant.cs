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
        public List<Department> Departments { get; set; } = new();
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int? CollegeId { get; set; }
        public College? College { get; set; }
        public bool InvolvesHumanOrAnimalSubjects { get; set; }
        public bool HasAbroadSupport { get; set; } // New field (grant field updates) 06/22
        public string Dissemination { get; set; } = string.Empty; // New field (grant field updates) 06/22
        public bool IsSaved { get; set; } // When false, the grant is submitted and ready for review. When true, it's still a draft?
        public List<GrantAttachment> Attachments { get; set; } = new();
        public List<BudgetItem> BudgetItems { get; set; } = new();
        public Review? Review { get; set; }
        public bool Acknowledgement1 { get; set; }
        public bool Acknowledgement2 { get; set; }
        public bool Acknowledgement3 { get; set; }
        public bool Acknowledgement4 { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Signature { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SignatureDate { get; set; }
        public bool? DeptReviewStatus { get; set; }
        public string DeptReviewNotes { get; set; } = string.Empty;

        public GrantStatus Status { get; set; } = GrantStatus.Saved;
    }

    public enum GrantStatus
    {
        [Display(Name = "Saved")]
        Saved,
        [Display(Name = "Submitted")]
        Submitted,
        [Display(Name = "Reviewed by dept chair")]
        ReviewedByDeptChair,
        [Display(Name = "Approved by dept chair")]
        ApprovedByDeptChair,
        [Display(Name = "Under review by ARCC")]
        UnderReviewByARCC,
        [Display(Name = "Approved ARCC")]
        ApprovedARCC
    }
}