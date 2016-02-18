using BudgetMaster.Models;
using BudgetMaster.Models.CodeFirst;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.Identity;

namespace BudgetMaster.HelperExtensions
{

    public class HelperExtensions
    {
    }

    public static class HHIDExtension
    {

        private static ApplicationDbContext db = new ApplicationDbContext();

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

    }

    public static class CatExtension
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
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

        //public static void UpdateAccountBalance(this Transaction transaction, string userId, Transaction OldTr)
        //{
        //    var user = db.Users.FirstOrDefault(u => u.Id.Equals(userId));
        //    var userHHID = Convert.ToInt32(user.HouseholdId);
        //    //var category = db.Categories.Find(userHHID);
        //    var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
        //    OldTr = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);

        //    if (OldTr.Category.Type == "Expense" && transaction.Category.Type == "Expense")
        //    {
        //        //SUBTRACT
        //        account.Balance += OldTr.Amount;
        //        account.Balance -= transaction.Amount;
        //    }
        //    else if (OldTr.Category.Type == "Income" && transaction.Category.Type == "Expense")
        //    {
        //        account.Balance -= OldTr.Amount;
        //        account.Balance -= transaction.Amount;
        //    }
        //    else if (OldTr.Category.Type == "Expense" && transaction.Category.Type == "Income")
        //    {
        //        account.Balance += OldTr.Amount;
        //        account.Balance += transaction.Amount;
        //    }
        //    else 
        //    {
        //        //ADD
        //        account.Balance -= OldTr.Amount;
        //        account.Balance += transaction.Amount;
        //    }
        //    db.SaveChanges();
        //}

        //public static void UpdateBalanceOnDelete(this Transaction transaction, string userId)
        //{
        //    var user = db.Users.Find(userId);
        //    var userHHID = user.HouseholdId;
        //    //var category = db.Categories.Find(userHHID);
        //    var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

        //    if (transaction.Category.Type == "Expense")
        //    {
        //        //ADD
        //        account.Balance += transaction.Amount;
        //    }
        //    else
        //    {
        //        //SUBTRACT
        //        account.Balance -= transaction.Amount;
        //    }
        //    db.SaveChanges();
        //    //return account.Balance;
        //}
    }

}