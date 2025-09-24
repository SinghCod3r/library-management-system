using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const decimal FINE_PER_DAY = 10.0m;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .ToListAsync();
                
            return View(transactions);
        }

        // GET: Transactions/IssueBook
        public IActionResult IssueBook()
        {
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "BookId", "Title");
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name");
            return View();
        }

        // POST: Transactions/IssueBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBook([Bind("BookId,MemberId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(transaction.BookId);
                if (book != null && book.IsAvailable)
                {
                    transaction.IssueDate = DateTime.Now;
                    transaction.DueDate = DateTime.Now.AddDays(14); // 2 weeks loan period
                    transaction.Fine = 0;

                    // Update book availability
                    book.IsAvailable = false;
                    _context.Update(book);
                    
                    _context.Add(transaction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Book is not available for issue.");
                }
            }
            
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "BookId", "Title", transaction.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", transaction.MemberId);
            return View(transaction);
        }

        // GET: Transactions/ReturnBook/5
        public async Task<IActionResult> ReturnBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
                
            if (transaction == null || transaction.ReturnDate != null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/ReturnBook/5
        [HttpPost, ActionName("ReturnBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBookConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null || transaction.ReturnDate != null)
            {
                return NotFound();
            }

            // Set return date
            transaction.ReturnDate = DateTime.Now;
            
            // Calculate fine if overdue
            if (DateTime.Now > transaction.DueDate)
            {
                int daysLate = (int)(DateTime.Now - transaction.DueDate).TotalDays;
                transaction.Fine = daysLate * FINE_PER_DAY;
            }

            // Update book availability
            var book = await _context.Books.FindAsync(transaction.BookId);
            if (book != null)
            {
                book.IsAvailable = true;
                _context.Update(book);
            }

            _context.Update(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Transactions/ActiveIssues
        public async Task<IActionResult> ActiveIssues()
        {
            var activeIssues = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .Where(t => t.ReturnDate == null)
                .ToListAsync();
                
            return View(activeIssues);
        }

        // GET: Transactions/OverdueBooks
        public async Task<IActionResult> OverdueBooks()
        {
            var today = DateTime.Now;
            var overdueBooks = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .Where(t => t.ReturnDate == null && t.DueDate < today)
                .ToListAsync();
                
            // Calculate potential fines
            foreach (var transaction in overdueBooks)
            {
                int daysLate = (int)(today - transaction.DueDate).TotalDays;
                transaction.Fine = daysLate * FINE_PER_DAY;
            }
                
            return View(overdueBooks);
        }
    }
}