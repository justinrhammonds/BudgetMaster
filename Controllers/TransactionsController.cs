using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BudgetMaster.Models;
using BudgetMaster.Models.CodeFirst;
using AspNetIdentity2.Controllers;
using BudgetMaster.HelperExtensions;
using Microsoft.AspNet.Identity;

namespace BudgetMaster.Controllers
{
    [RequireHttps]
    [AuthorizeHouseholdRequired]
    public class TransactionsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {

            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Include("Accounts").FirstOrDefault(h => h.Id == user.HouseholdId);

            if (household == null)
            {
                TempData["FirstCreateOrJoin"] = "To Get Started, You Must First Create or Join a Group.";
                return RedirectToAction("Create", "Households");
            }
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var transactions = db.Transactions.Where(t => t.AccountId == t.Account.Id && t.Account.HouseholdId == userHHID);
            var model = transactions.OrderByDescending(d => d.PostedDate).ToList();
            return View(model);
        }

        // GET: Transactions/Create
        public PartialViewResult _CreatePV(int? id) 
        {
            //returns a partial view containing a list of HH accounts and a list of HH categories (for dropdowns
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var accounts = db.Accounts.Where(a => a.HouseholdId == userHHID && a.IsDeleted == false);
            var categories = db.Categories.Where(c => c.HouseholdId == userHHID && c.IsDeleted == false);
            
            ViewBag.AccountId = new SelectList(accounts.ToList(), "Id", "Name");
            ViewBag.CategoryId = new SelectList(categories.ToList(), "Id", "Name");
            return PartialView();
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PostedDate,Amount,Reconciled,Description,CategoryId,AccountId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);

                transaction.PostedById = userId;
                db.Transactions.Add(transaction); 
                db.SaveChanges();

                transaction = db.Transactions.Include("Category").FirstOrDefault(t => t.Id == transaction.Id);
                transaction.UpdateAccountBalance(userId);
                return RedirectToAction("Index", "Transactions", new { id = account.Id });
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public PartialViewResult _EditPV(int? id)
        {
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var categories = db.Categories.Where(c => c.HouseholdId == userHHID && c.IsDeleted == false);
            Transaction transaction = db.Transactions.Find(id);
            ViewBag.CategoryId = new SelectList(categories.ToList(), "Id", "Name", transaction.CategoryId);
            return PartialView(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PostedDate,Amount,Reconciled,Description,CategoryId,AccountId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var account = db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
                var OldTr = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);
                OldTr.ReverseAccountBalance(userId);
               
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                transaction = db.Transactions.Include("Category").FirstOrDefault(t => t.Id == transaction.Id);
                transaction.UpdateAccountBalance(userId);
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public PartialViewResult _DeleteTranPV(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            return PartialView(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var userId = User.Identity.GetUserId();
            Transaction transaction = db.Transactions.Find(id);
            transaction.ReverseAccountBalance(userId);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
