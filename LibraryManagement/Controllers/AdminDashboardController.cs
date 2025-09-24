using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get counts for dashboard
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.AvailableBooks = await _context.Books.CountAsync(b => b.IsAvailable);
            ViewBag.TotalMembers = await _context.Members.CountAsync();
            ViewBag.ActiveIssues = await _context.Transactions.CountAsync(t => t.ReturnDate == null);
            ViewBag.OverdueBooks = await _context.Transactions.CountAsync(t => t.ReturnDate == null && t.DueDate < System.DateTime.Today);
            
            // Get recent transactions for dashboard
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .OrderByDescending(t => t.IssueDate)
                .Take(5)
                .ToListAsync();
                
            return View();
        }
    }
}