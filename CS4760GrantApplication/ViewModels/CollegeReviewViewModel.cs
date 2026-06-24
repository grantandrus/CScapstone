using CS4760GrantApplication.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS4760GrantApplication.ViewModels
{
    public class CollegeReviewViewModel
    {
        public Grant Grant { get; set; } = null!;
        public string Notes { get; set; } = string.Empty;
    }
}
