using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models.CodeFirst
{
    public class Invite
    {
        public int Id { get; set; }

        public string GeneratedCode { get; set; }
        [Required]
        public string InvitedUser { get; set; }

        public int HouseholdId { get; set; }

        public virtual Household Household { get; set;}


    }
}