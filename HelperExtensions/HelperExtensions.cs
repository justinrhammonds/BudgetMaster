using BudgetMaster.Models;
using BudgetMaster.Models.CodeFirst;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace BudgetMaster.HelperExtensions
{

    public static class HelperExtensions
    {
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }

    public static class HHIDExtension
    {

        private static ApplicationDbContext db = new ApplicationDbContext();

        //returns the HHID of the current user (from cookie)
        public static string GetHouseholdId(this IIdentity user)
        {
            var ClaimUser = (ClaimsIdentity)user;
            var Claim = ClaimUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            if (Claim != null)
            {
                return Claim.Value;
            }
            else
            {
                return null;
            }
        }

        public static bool IsInHousehold(this IIdentity user)
        {
            var cUser = (ClaimsIdentity)user;
            var hid = cUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            return (hid != null && !string.IsNullOrWhiteSpace(hid.Value));
        }
    }

    public static class CatExtension
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        //populates a list of pre-defined categories from the CategoryList Model
        public static void PopulateCategories(this Household household)
        {
            var Source = db.CategoryLists.ToList();
            var categoryList = new List<Category>();
           
            foreach (var cat in Source)
            {
                var category = new Category()
                {
                    Name = cat.Name,
                    HouseholdId = household.Id,
                    Type = cat.Type
                };
                categoryList.Add(category);
            }
            db.Categories.AddRange(categoryList);
            db.SaveChanges();
        }
    }

    public static class BalanceExtensions
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        // updates the account.Balance based on the transaction.Category.Type
        // used in Transactions/Create and Transactions/Edit ActionResults
        public static void UpdateAccountBalance(this Transaction transaction, string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id.Equals(userId));
            var userHHID = Convert.ToInt32(user.HouseholdId);
            var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

            if (transaction.Category.Type == "Expense")
            {
                account.Balance -= transaction.Amount; // SUBTRACT
            }
            else
            {
                account.Balance += transaction.Amount; // ADD
            }

            db.SaveChanges();
        }

        // updates (reverts) the account.Balance based on the transaction.Category.Type
        // used in Transactions/Edit and Transactions/Delete ActionResults
        public static void ReverseAccountBalance(this Transaction transaction, string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id.Equals(userId));
            var userHHID = Convert.ToInt32(user.HouseholdId);
            var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

            if (transaction.Category.Type == "Expense")
            {
                account.Balance += transaction.Amount; // ADD BACK
            }
            else
            {
                account.Balance -= transaction.Amount; // SUBTRACT BACK
            }

            db.SaveChanges();
        }

        public static decimal GetBalance(this decimal amount, string type)
        {
            decimal Balance = 0;
            if (type == "Expense")
            {
                Balance -= amount;
            }
            else
            {
                Balance += amount;
            }
            return Balance;
        }
    }

}