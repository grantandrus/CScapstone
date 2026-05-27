using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class GrantAttachment
    {
        public int Id { get; set; }

        public int GrantId { get; set; }
        public Grant? Grant { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public bool IsApprovalFile { get; set; }
    }
}