using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const decimal FINE_PER_DAY = 10.0m;

        public TransactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
                        IQueryable<Transaction> transactionsQuery = _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member);
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == currentUser.Email);
                    if (member != null)
                    {
                        transactionsQuery = transactionsQuery.Where(t => t.MemberId == member.MemberId);
                    }
                    else
                    {
                        transactionsQuery = transactionsQuery.Where(t => false);
                    }
                }
            }

            var transactions = await transactionsQuery.ToListAsync();

            return View(transactions);
        }

        // GET: Transactions/IssueBook
        public async Task<IActionResult> IssueBook(int? bookId = null)
        {
            var isAdmin = User.IsInRole("Admin");
            int? preselectedMemberId = null;
            Book? preselectedBook = null;
            Member? preselectedMember = null;

            if (bookId.HasValue)
            {
                preselectedBook = await _context.Books.FirstOrDefaultAsync(b => b.BookId == bookId && b.IsAvailable);
                if (preselectedBook == null)
                {
                    TempData["ErrorMessage"] = "Selected book is not available.";
                    return RedirectToAction("Index", "Books");
                }
            }

            if (!isAdmin)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    preselectedMember = await _context.Members.FirstOrDefaultAsync(m => m.Email == currentUser.Email);
                    if (preselectedMember != null)
                    {
                        preselectedMemberId = preselectedMember.MemberId;
                    }
                    else
                    {
                        // Auto-create a member profile for the logged-in user if none exists
                        preselectedMember = new Member
                        {
                            Name = currentUser.Email!, // default name as email
                            Email = currentUser.Email!,
                            MembershipType = "Standard",
                            ExpiryDate = DateTime.Now.AddYears(1)
                        };
                        _context.Members.Add(preselectedMember);
                        await _context.SaveChangesAsync();
                        preselectedMemberId = preselectedMember.MemberId;
                        TempData["SuccessMessage"] = "A new member profile was created for your account.";
                    }
                }
            }

            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "BookId", "Title", bookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "Name", preselectedMemberId);
            ViewBag.PreselectedBook = preselectedBook;
            ViewBag.PreselectedMember = preselectedMember;

            return View(new Transaction { BookId = bookId ?? 0, MemberId = preselectedMemberId ?? 0 });
        }

        // POST: Transactions/IssueBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBook([Bind("BookId,MemberId,DueDate")] Transaction transaction)
        {
            // Validate required fields
            if (transaction.BookId == 0)
                ModelState.AddModelError("BookId", "Please select a book");
            if (transaction.MemberId == 0)
                ModelState.AddModelError("MemberId", "Please select a member");

            var today = DateTime.Today;
            if (transaction.DueDate == default)
            {
                ModelState.AddModelError("DueDate", "Please choose a return date");
            }
            else if (transaction.DueDate < today)
            {
                ModelState.AddModelError("DueDate", "Return date cannot be before today");
            }
            else if (transaction.DueDate > today.AddDays(15))
            {
                ModelState.AddModelError("DueDate", "Return date cannot be more than 15 days from today");
            }

            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(transaction.BookId);
                if (book != null && book.IsAvailable)
                {
                    transaction.IssueDate = DateTime.Now;
                    transaction.Fine = 0;

                    // Update book availability
                    book.IsAvailable = false;
                    _context.Update(book);

                    _context.Add(transaction);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Book issued successfully!";
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