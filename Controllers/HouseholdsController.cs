using System.Data;
using System.Linq;
using System.Web.Mvc;
using BudgetMaster.Models;
using BudgetMaster.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using AspNetIdentity2.Controllers;
using BudgetMaster.HelperExtensions;
using System.Collections;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BudgetMaster.Controllers
{
    [RequireHttps]
    [Authorize]
    public class HouseholdsController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: Households/Index/5
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Include("Accounts").FirstOrDefault(h => h.Id == user.HouseholdId);


            if (household != null)
            {
                var ReconciledBalance = household.Accounts.SelectMany(a => a.Transactions).Where(t => t.Reconciled == true).Select(m => m.Amount).Sum();

                var accountList = from acc in household.Accounts
                                  let inc = (from tr in acc.Transactions
                                             where (tr.Reconciled == true && tr.Category.Type == "Income")
                                             select tr.Amount).Sum()
                                  let exp = (from tr in acc.Transactions
                                             where (tr.Reconciled == true && tr.Category.Type == "Expense")
                                             select tr.Amount).Sum()
                                  select new RecBalVM()
                                  {
                                      Account = acc,
                                      RecBal = inc - exp
                                  };

                DashboardVM dashboardVM = new DashboardVM()
                {
                    Household = household,
                    RecBalVM = accountList.ToList()
                };

                

                return View(dashboardVM);
            }

            return RedirectToAction("Create", "Households");

        }

        //GET: Households/Manage
        public ActionResult Manage()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Household household = db.Households.Include("Accounts").FirstOrDefault(h => h.Id == user.HouseholdId);

            if (household == null)
            {
                return RedirectToAction("Create");
            }

            ViewBag.MessageSuccess = TempData["MessageSuccess"];
            return View(household);
        }

        
        // GET: Households/GetCharts
        public ActionResult GetCharts()
        {
            var id = User.Identity.GetHouseholdId();
            var hhid = Convert.ToInt32(id);
            var hh = db.Households.Find(hhid);

            var catBarList = (from cat in hh.Categories
                              where cat.Type == "Expense" && cat.IsDeleted == false
                              let sumBud = (from bud in cat.BudgetItems
                                            select bud.Amount
                                            ).DefaultIfEmpty().Sum()
                              let sumAct = (from tran in cat.Transactions
                                            where tran.PostedDate.Year == DateTime.Now.Year &&
                                            tran.PostedDate.Month == DateTime.Now.Month 
                                            select tran.Amount).DefaultIfEmpty().Sum()
                              select new
                              {
                                  category = cat.Name,
                                  budgeted = sumBud,
                                  actual = sumAct
                              }).ToArray();

            var inc = db.Transactions.Where(t => t.Account.HouseholdId == hh.Id &&
                                            t.Category.Type == "Income" &&
                                            t.Account.IsDeleted == false &&
                                            t.PostedDate.Year == DateTime.Now.Year &&
                                            t.PostedDate.Month == DateTime.Now.Month)
                                            .Select(t => t.Amount).DefaultIfEmpty().Sum();
            var exp = db.Transactions.Where(t => t.Account.HouseholdId == hh.Id &&
                                            t.Category.Type == "Expense" &&
                                            t.Account.IsDeleted == false &&
                                            t.PostedDate.Year == DateTime.Now.Year &&
                                            t.PostedDate.Month == DateTime.Now.Month)
                                            .Select(t => t.Amount).DefaultIfEmpty().Sum();

            var donutList = new[] {  new { label = "Income", value = (int)inc },
                                new { label = "Expenses", value = (int)exp } };
            var data = new
            {
                donutList = donutList,
                carBarList = catBarList
            };

            return Content(JsonConvert.SerializeObject(data), "application/json");
        }

        // GET: Households/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Households/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user.HouseholdId == null)
                {
                    db.Households.Add(household);
                    db.SaveChanges();
                    var hh = db.Households.FirstOrDefault(h => h.Name == household.Name);
                    //user = db.Users.Find(User.Identity.GetUserId());
                    user.HouseholdId = hh.Id;
                    household.PopulateCategories();
                    db.SaveChanges();

                    await ControllerContext.HttpContext.RefreshAuthentication(user);
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
            }

            return View(household);
        }

        // POST: Households/Invite
        [HttpPost]
        [AuthorizeHouseholdRequired]
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
                im.Body = "You've been invited to join a group by " + user.FirstName + " " + user.LastName + ". Visit https://jhammonds-budgetmaster.azurewebsites.net to register. Once registered, enter the following code to join: " + Code + "";
                im.Destination = invite.InvitedUser;
                im.Subject = "You're Invited to BudgetMaestro";
                es.SendAsync(im);
                TempData["MessageSuccess"] = "Your Message Was Sent Successfully.";
                //ViewBag.MessageSuccess = "Your Message Was Sent Successfully.";
                return RedirectToAction("Manage", "Households");
            }

            return View();
        }

        // GET: Households/Join/5
        public ActionResult Join()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Join(string Code)
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
                    await ControllerContext.HttpContext.RefreshAuthentication(user);
                    return RedirectToAction("Index", "Households", new { id = user.HouseholdId });
                }
                
                return RedirectToAction("Index", "Households"); 
            }
            return View();
        }

        // GET: Households/_LeavePV
        public PartialViewResult _LeavePV()
        {
            return PartialView();
        }

        // POST: Households/Leave
        [HttpPost]
        [AuthorizeHouseholdRequired]
        public async Task<ActionResult> Leave()
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                user.HouseholdId = null;
                db.SaveChanges();

                await ControllerContext.HttpContext.RefreshAuthentication(user);

                return RedirectToAction("Create", "Households");
            }
            return View();
        }
    }
}
