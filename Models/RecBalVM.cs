using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BudgetMaster.Models.CodeFirst;

namespace BudgetMaster.Models
{
    public class RecBalVM
    {
        public Account Account { get; set; }
        public decimal RecBal { get; set; }
    }
}