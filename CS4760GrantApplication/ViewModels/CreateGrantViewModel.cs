using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.ViewModels
{
    public class CreateGrantViewModel
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

        public int? UserId { get; set; }
        public int CollegeId { get; set; }

        public bool InvolvesHumanOrAnimalSubjects { get; set; }
        public bool IsSaved { get; set; }

        public List<GrantAttachment> Attachments { get; set; } = new();
        public List<int> SelectedDepartmentIds { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Users { get; set; } = new();
        public List<SelectListItem> Colleges { get; set; } = new();
        public string BudgetDescription { get; set; } = string.Empty;

        public string BudgetItemType { get; set; } = string.Empty;

        public string BudgetFundingSource { get; set; } = string.Empty;

        public decimal? BudgetAmount { get; set; }

        // Tracking IDs for attachments that should be deleted
        public List<int> AttachmentsToRemove { get; set; } = new();
        public List<BudgetItem> BudgetItems { get; set; } = new();
        public bool Acknowledgement1 { get; set; }
        public bool Acknowledgement2 { get; set; }
        public bool Acknowledgement3 { get; set; }
        public bool Acknowledgement4 { get; set; }
        [Required(AllowEmptyStrings = true)]
        public String Signature { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SignatureDate { get; set; }
        public bool? DeptReviewStatus { get; set; }
    }
}
