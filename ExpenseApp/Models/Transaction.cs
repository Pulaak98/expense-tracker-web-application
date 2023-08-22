using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseApp.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1,int.MaxValue,ErrorMessage ="please select category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "amount must be greater than zero")]
        public int Amount { get; set; }

        [Column(TypeName="nvarchar(75)") ]
        public string? Note { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public string? CatergoryWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type=="Expense") ? "- " : "+ ")+Amount.ToString();
            }
        }
    }
}
