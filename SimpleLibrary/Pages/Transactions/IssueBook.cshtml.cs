using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Data;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Authorization;

namespace SimpleLibrary.Pages.Transactions
{
    [Authorize(Roles = "Admin")]
    public class IssueBookModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IssueBookModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;

        public SelectList Books { get; set; } = default!;
        public SelectList Members { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Books = new SelectList(await _context.Books.Where(b => b.IsAvailable).ToListAsync(), "Id", "Title");
            Members = new SelectList(await _context.Members.ToListAsync(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Books = new SelectList(await _context.Books.Where(b => b.IsAvailable).ToListAsync(), "Id", "Title");
                Members = new SelectList(await _context.Members.ToListAsync(), "Id", "Name");
                return Page();
            }

            // Validate DueDate
            if (Transaction.DueDate <= DateTime.Today)
            {
                ModelState.AddModelError("Transaction.DueDate", "Due date must be after today.");
                Books = new SelectList(await _context.Books.Where(b => b.IsAvailable).ToListAsync(), "Id", "Title");
                Members = new SelectList(await _context.Members.ToListAsync(), "Id", "Name");
                return Page();
            }

            if (Transaction.DueDate > DateTime.Today.AddDays(15))
            {
                ModelState.AddModelError("Transaction.DueDate", "Due date cannot be more than 15 days from today.");
                Books = new SelectList(await _context.Books.Where(b => b.IsAvailable).ToListAsync(), "Id", "Title");
                Members = new SelectList(await _context.Members.ToListAsync(), "Id", "Name");
                return Page();
            }

            Transaction.IssueDate = DateTime.Today;
            Transaction.ReturnDate = null;
            Transaction.Fine = 0;

            // Update book availability
            var book = await _context.Books.FindAsync(Transaction.BookId);
            if (book != null)
            {
                book.IsAvailable = false;
            }

            _context.Transactions.Add(Transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book issued successfully!";
            return RedirectToPage("./Index");
        }
    }
}