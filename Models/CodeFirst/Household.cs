using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models.CodeFirst
{
    public class Household
    {
        public Household()
        {
            this.Accounts = new HashSet<Account>();
            this.Categories = new HashSet<Category>();
            this.BudgetItems = new HashSet<BudgetItem>();
            this.Invites = new HashSet<Invite>();
            this.Users = new HashSet<ApplicationUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Category>Categories { get; set; }
        public virtual ICollection <BudgetItem> BudgetItems { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}