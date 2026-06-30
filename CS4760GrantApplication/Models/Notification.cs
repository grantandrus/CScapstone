using System.ComponentModel.DataAnnotations;

namespace CS4760GrantApplication.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int GrantId { get; set; }
        public Grant? Grant { get; set; }

        [Required]
        [StringLength(300)]
        public string Message { get; set; } = string.Empty;

        public DateTime NotificationDate { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
