using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetMaster.Models
{
    public class StringUtilities
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}