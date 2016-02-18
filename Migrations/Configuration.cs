namespace BudgetMaster.Migrations
{
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

        protected override void Seed(BudgetMaster.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.CategoryLists.AddRange(
                new List<CategoryList>() {
                new CategoryList { Name = "Paycheck", Type = "Income"},
                new CategoryList { Name = "Predictable Bonus", Type = "Income"},
                new CategoryList { Name = "Expense Reimbursement", Type = "Income"},
                new CategoryList { Name = "Investment", Type = "Income"},
                new CategoryList { Name = "Miscellaneous", Type = "Income"},
                new CategoryList { Name = "Automobile", Type = "Expense"},
                new CategoryList { Name = "Bank Charges", Type = "Expense"},
                new CategoryList { Name = "Charity", Type = "Expense"},
                new CategoryList { Name = "Childcare", Type = "Expense"},
                new CategoryList { Name = "Clothing", Type = "Expense"},
                new CategoryList { Name = "Credit Card Fees", Type = "Expense"},
                new CategoryList { Name = "Education", Type = "Expense"},
                new CategoryList { Name = "Events", Type = "Expense"},
                new CategoryList { Name = "Food", Type = "Expense"},
                new CategoryList { Name = "Gifts", Type = "Expense"},
                new CategoryList { Name = "Healthcare", Type = "Expense"},
                new CategoryList { Name = "Household", Type = "Expense"},
                new CategoryList { Name = "Insurance", Type = "Expense"},
                new CategoryList { Name = "Job Expenses", Type = "Expense"},
                new CategoryList { Name = "Leisure(Daily/Non-Vacation)", Type = "Expense"},
                new CategoryList { Name = "Hobbies", Type = "Expense"},
                new CategoryList { Name = "Loans", Type = "Expense"},
                new CategoryList { Name = "Pet Care", Type = "Expense"},
                new CategoryList { Name = "Savings", Type = "Expense"},
                new CategoryList { Name = "Taxes", Type = "Expense"},
                new CategoryList { Name = "Utilities", Type = "Expense"},
                new CategoryList { Name = "Vacation", Type = "Expense"}
                });
        }
    }
}
