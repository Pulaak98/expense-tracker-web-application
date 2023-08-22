using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {

            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            List<Transaction> selectedTransaction = await _context.Transactions
                 .Include(x => x.Category)
                 .Where(y => y.Date >= startDate && y.Date <= endDate)
                 .ToListAsync();

            int totalIncome = selectedTransaction
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.totalIncome = totalIncome.ToString("C0");

            int totalExpense = selectedTransaction
               .Where(i => i.Category.Type == "Expense")
               .Sum(j => j.Amount);
            ViewBag.totalExpense = totalExpense.ToString("C0");

            int balance = totalIncome - totalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.balance = String.Format(culture, "{0:C0}", balance);

            ViewBag.doughnutChartData = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0")
                })
                .OrderByDescending(l=>l.amount)
                .ToList();

            List<SplineChartData> incomeSummary = selectedTransaction
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            List<SplineChartData> expenseSummary = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            string[] last7Days = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.splineChartData = from day in last7Days
                                      join income in incomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in expenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense
                                      };

            ViewBag.recentTransactions =  _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
               .ToList();

            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
