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
    [AuthorizeHouseholdRequired]
    public class BudgetItemsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItems
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Include("Accounts").FirstOrDefault(h => h.Id == user.HouseholdId);

            if (household == null)
            {
                return RedirectToAction("Create", "Households");
            }
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var budgetItems = db.BudgetItems.Where(t => t.HouseholdId == userHHID);
            var model = budgetItems.OrderByDescending(b => b.Amount).ToList();
            return View(model);
        }

        // GET: BudgetItems/Create
        public PartialViewResult _CreatePV()
        {
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var categories = db.Categories.Where(c => c.HouseholdId == userHHID && c.IsDeleted == false);
            ViewBag.CategoryId = new SelectList(categories.ToList(), "Id", "Name");
            return PartialView();
        }

        // POST: BudgetItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Amount,CategoryId,HouseholdId")] BudgetItem budgetItem)
        {
            if (ModelState.IsValid)
            {
                var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
                budgetItem.HouseholdId = userHHID;
                db.BudgetItems.Add(budgetItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", budgetItem.CategoryId);
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budgetItem.HouseholdId);
            return View(budgetItem);
        }

        // GET: BudgetItems/Edit/5
        public PartialViewResult _EditPV(int? id)
        {
            var userHHID = Convert.ToInt32(User.Identity.GetHouseholdId());
            var categories = db.Categories.Where(c => c.HouseholdId == userHHID && c.IsDeleted == false);
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            ViewBag.CategoryId = new SelectList(categories.ToList(), "Id", "Name", budgetItem.CategoryId);
            return PartialView(budgetItem);
        }

        // POST: BudgetItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Amount,Frequency,CategoryId,HouseholdId")] BudgetItem budgetItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budgetItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", budgetItem.CategoryId);
            return View(budgetItem);
        }

        // GET: BudgetItems/Delete/5
        public PartialViewResult _DeleteBiPV(int? id)
        {
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            return PartialView(budgetItem);
        }

        // POST: BudgetItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            db.BudgetItems.Remove(budgetItem);
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

