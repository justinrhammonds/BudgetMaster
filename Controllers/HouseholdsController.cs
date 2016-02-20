using System.Data;
using System.Linq;
using System.Web.Mvc;
using BudgetMaster.Models;
using BudgetMaster.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using AspNetIdentity2.Controllers;
using BudgetMaster.HelperExtensions;

namespace BudgetMaster.Controllers
{
    [RequireHttps]
    [Authorize]
    public class HouseholdsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        // GET: Households/Details/5
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Include("Accounts").FirstOrDefault(h => h.Id == user.HouseholdId);
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
                    household.PopulateCategories();
                    db.SaveChanges();

                    return RedirectToAction("Index", new { id = hh.Id });
                }
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
                Invite invite = new Invite();
                var user = db.Users.Find(User.Identity.GetUserId());
                invite.HouseholdId = (int)user.HouseholdId;
                invite.InvitedUser = Email;
                var duplicates = db.Invites.Where(i => i.InvitedUser == Email);
                foreach (var duplicate in duplicates)
                {
                    db.Invites.Remove(duplicate);
                }
                var Code = StringUtilities.RandomString(6);
                invite.GeneratedCode = Code;
                db.Invites.Add(invite);
                db.SaveChanges();
                EmailService es = new EmailService();
                var im = new IdentityMessage();
                im.Body = "You've been invited to join a household by " + user.FirstName + " " + user.LastName + ". Visit https://jhammonds-budgetmaster.azurewebsites.net to register. Once registered, enter the following code to join: " + Code + "";
                im.Destination = invite.InvitedUser;
                im.Subject = "You're Invited to BudgetMaster";
                es.SendAsync(im);
                ViewBag.Message = "Your Message Was Sent Successfully.";

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
        public ActionResult Join(string Code)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var invitation = db.Invites.FirstOrDefault(i => i.GeneratedCode == Code && i.InvitedUser == user.Email);
                if (invitation != null)
                {
                    user.HouseholdId = invitation.HouseholdId;
                    db.Invites.Remove(invitation);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
                }
                return RedirectToAction("Index", "Households"); 
            }
            return View();
        }

        // GET: Households/_LeavePV
        [HttpGet]
        public PartialViewResult _LeavePV()
        {
            return PartialView();
        }

        [HttpPost]
        // POST: Households/Leave
        public ActionResult Leave()
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                user.HouseholdId = null;
                db.SaveChanges();
                return RedirectToAction("Create", "Households");
            }
            return View();
        }
    }
}
