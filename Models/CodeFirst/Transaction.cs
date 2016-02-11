using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models.CodeFirst
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public DateTimeOffset PostedDate { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public bool Type { get; set; }
        public bool Reconciled { get; set; }
        public string Description { get; set; }

        public int? CategoryId { get; set; }
        public int AccountId { get; set; }
        
        public virtual Category Category { get; set; }
        public virtual Account Account { get; set; }

    }
}