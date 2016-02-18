using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models.CodeFirst
{
    public class Category   
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        public int? HouseholdId { get; set; }

        public virtual Household Household { get; set; }
    }
}