using BudgetMaster.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models
{
    public class DashboardVM
    {
        public Household Household { get; set; }
        public List<RecBalVM> RecBalVM { get; set; }
    }
}