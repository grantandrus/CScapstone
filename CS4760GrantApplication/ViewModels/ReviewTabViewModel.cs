using CS4760GrantApplication.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS4760GrantApplication.ViewModels
{
    public class ReviewTabViewModel
    {
        public List<Grant> Grants { get; set; } = new();
    }
}
