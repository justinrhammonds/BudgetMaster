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
using Microsoft.AspNet.Identity;

namespace BudgetMaster.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        // GET: Households/Details/5
        public ActionResult Index()
        {
            //these two lines will go in every controller...
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Find(user.HouseholdId);
            ///////////////////////////////////////////////////////////

            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        [HttpGet]
        // GET: Households/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user.HouseholdId == null)
                {
                    db.Households.Add(household);
                    db.SaveChanges();
                    var hh = db.Households.FirstOrDefault(h => h.Name == household.Name);
                    user = db.Users.Find(User.Identity.GetUserId());
                    user.HouseholdId = hh.Id;
                    db.SaveChanges();

                    return RedirectToAction("Index", new { id = hh.Id });
                }
                //change to throw an error message up "You need to leave your current household before creating a new one."
                return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
            }

            return View(household);
        }

        // POST: Households/Invite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invite(string Email)
        {

            if (ModelState.IsValid)
            {
                Invite invite = new Models.CodeFirst.Invite();
                var user = db.Users.Find(User.Identity.GetUserId());
                //Household household = db.Households.Find(user.HouseholdId);
                invite.HouseholdId = (int)user.HouseholdId;

                invite.InvitedUser = Email;

                var Code = StringUtilities.RandomString(6);
                invite.GeneratedCode = Code;

                db.Invites.Add(invite);
                db.SaveChanges();
                return RedirectToAction("Index", "Households");
            }

            return View();
        }

        [HttpGet]
        // GET: Households/Join/5
        public ActionResult Join()
        {
            return View();
        }

        [HttpPost]
        // POST: Households/Join/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        public ActionResult Join(string Code)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var invitation = db.Invites.FirstOrDefault(i => i.GeneratedCode == Code && i.InvitedUser == user.Email);

                if (invitation != null)
                {
                    user.HouseholdId = invitation.HouseholdId;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
                }
                
                return RedirectToAction("Index", "Households"); 

            }

            return View();
        }

        [HttpGet]
        // GET: Households/Leave
        public ActionResult Leave()
        {
            return View();
        }

        [HttpPost]
        // POST: Households/Leave
        public ActionResult Leave(int? Id)
        {
            if (ModelState.IsValid)
            {

                var user = db.Users.Find(User.Identity.GetUserId());
                user.HouseholdId = null;
                db.SaveChanges();

                return RedirectToAction("Create");

            }

            return View();
        }
    }
}
