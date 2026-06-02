using CS4760GrantApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace CS4760GrantApplication.ViewModels
{
    public class RubricCreateViewModel
    {
        public List<RubricCriterion> RubricCriteria { get; set; }
    }
}
