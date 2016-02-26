namespace BudgetMaster.Migrations
{
    using Models;
    using Models.CodeFirst;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BudgetMaster.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
        //private ApplicationDbContext db = new ApplicationDbContext();
        protected override void Seed(BudgetMaster.Models.ApplicationDbContext context)
        {

            //context.CategoryLists.RemoveRange(db.CategoryLists);
            context.CategoryLists.AddRange(
                new List<CategoryList>() {
                new CategoryList { Name = "Paycheck", Type = "Income"},
                new CategoryList { Name = "Misc. Income", Type = "Income"},
                new CategoryList { Name = "Bills", Type = "Expense"},
                new CategoryList { Name = "Food", Type = "Expense"},
                new CategoryList { Name = "Clothing", Type = "Expense"},
                new CategoryList { Name = "Auto", Type = "Expense"},
                new CategoryList { Name = "Childcare", Type = "Expense"},
                new CategoryList { Name = "Education", Type = "Expense"},
                new CategoryList { Name = "Household", Type = "Expense"},
                new CategoryList { Name = "Healthcare", Type = "Expense"},
                new CategoryList { Name = "Misc. Expense", Type = "Expense"}
                });
        }
    }
}
