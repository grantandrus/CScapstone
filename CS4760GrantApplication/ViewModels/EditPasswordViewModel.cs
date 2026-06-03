using CS4760GrantApplication.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS4760GrantApplication.ViewModels
{
    public class EditPasswordViewModel
    {
        public int UserID { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}