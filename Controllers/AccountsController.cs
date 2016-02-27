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

namespace BudgetMaster
{
    [RequireHttps]
    [Authorize]
    public class AccountsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        [Authorize]
        public PartialViewResult _AccIndexPV()
        {
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var accounts = db.Accounts.Where(a => a.HouseholdId == userHHID && a.IsDeleted == false);
            var model = accounts.ToList();
            ViewBag.HouseholdId = userHHID;
            return PartialView(model);
        }

        // GET: Accounts/Details/5
        public PartialViewResult _AccDetailsPV(int? id)
        {
            Account account = db.Accounts.Find(id);
            //var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var transactions = db.Transactions.Where(t => t.AccountId == account.Id).ToList();

            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //if (account == null)
            //{
            //    return HttpNotFound();
            //}

            var ReconciledTransactions = transactions.Where(t => t.Reconciled == true);
            decimal ReconciledBalance = 0;
            foreach (var t in ReconciledTransactions)
            {
                if (t.Category.Type == "Expense")
                {
                    ReconciledBalance -= t.Amount;
                } else
                {
                    ReconciledBalance += t.Amount;
                }
            }
            ViewBag.ReconciledBalance = ReconciledBalance;

            return PartialView(account);
        }

        // GET: Accounts/Create
        public PartialViewResult _CreatePV()
        {
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name");
            return PartialView();
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Balance,HouseholdId")] Account account)
        {
            if (ModelState.IsValid)
            {
                var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
                var house = db.Households.Find(userHHID);
                account.HouseholdId = userHHID;
                db.Accounts.Add(account);
                db.SaveChanges();
                Transaction transaction = new Transaction()
                {
                    PostedDate = DateTimeOffset.Now,
                    Amount = account.Balance,
                    Reconciled = true,
                    Description = "Initial Deposit",
                    CategoryId = house.Categories.FirstOrDefault(c=>c.Name == "Misc. Income").Id,
                    PostedById = User.Identity.GetUserId(),
                    AccountId = account.Id
                };
                db.Transactions.Add(transaction);
                db.SaveChanges();

                return RedirectToAction("Index", "Transactions");
            }

            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", account.HouseholdId);
            return View(account);
        }

        // GET: Accounts/Edit/5
        public PartialViewResult _EditPV(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Account account = db.Accounts.Find(id);
            //if (account == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(account);
        }

        // POST: Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Balance,HouseholdId")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Transactions");
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", account.HouseholdId);
            return View(account);
        }

        // GET: Accounts/Delete/5
        public PartialViewResult _DeletePV(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            Account account = db.Accounts.Find(id);
            //if (account == null)
            //{
            //    return HttpNotFound();
            //}
            return PartialView(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            account.IsDeleted = true;
            //db.Accounts.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index", "Transactions");
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
