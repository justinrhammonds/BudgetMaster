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
    [Authorize]
    public class TransactionsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            //returns a view with a list of transactions where the t's accountId matches the user's HHID
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var transactions = db.Transactions.Where(t => t.AccountId == t.Account.Id && t.Account.HouseholdId == userHHID);
            var model = transactions.OrderByDescending(d => d.PostedDate).ToList();
            return View(model);
        }

        //// GET: Transactions/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Transaction transaction = db.Transactions.Find(id);
        //    if (transaction == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(transaction);
        //}

        // GET: Transactions/Create
        public PartialViewResult _CreatePV()
        {
            //returns a partial view containing a list of HH accounts and a list of HH categories (for dropdowns
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var accounts = db.Accounts.Where(a => a.HouseholdId == userHHID);
            var categories = db.Categories.Where(c => c.HouseholdId == userHHID);
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
                //set the PostedBy value in the model
                transaction.PostedById = userId;
                db.Transactions.Add(transaction); //Add a new transaction record to model (db)
                db.SaveChanges();
                //fetch back the same transaction
                transaction = db.Transactions.Include("Category").FirstOrDefault(t => t.Id == transaction.Id);
                //update the balance based on the transaction.Category.Type
                transaction.UpdateAccountBalance(userId);
                return RedirectToAction("Index");
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public PartialViewResult _EditPV(int? id)
        {
            //returns a partial view (for this particular transaction) containing a list of HH accounts and a list of HH categories (for dropdowns
            Transaction transaction = db.Transactions.Find(id);
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
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
                //create a record of the transaction and store it in the cookie
                //then update (revert) account balance to it's original amount.
                OldTr.ReverseAccountBalance(userId);
               
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                //fetch the reverted transaction amount, and update it further.
                transaction = db.Transactions.Include("Category").FirstOrDefault(t => t.Id == transaction.Id);
                transaction.UpdateAccountBalance(userId);
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name", transaction.AccountId);
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", transaction.CategoryId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public PartialViewResult _DeletePV(int? id)
        {
            //returns a partial view containing properties for this particular transaction
            Transaction transaction = db.Transactions.Find(id);
            return PartialView(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //fetch a transaction (by it's particular id), then update it's account balance, and finally remove the transaction from the model record.
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
