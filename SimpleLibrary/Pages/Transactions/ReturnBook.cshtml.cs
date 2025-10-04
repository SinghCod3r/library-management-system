using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Data;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Authorization;

namespace SimpleLibrary.Pages.Transactions
{
    [Authorize(Roles = "Admin")]
    public class ReturnBookModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReturnBookModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Transaction Transaction { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .FirstOrDefaultAsync(t => t.Id == id && t.ReturnDate == null);

            if (transaction == null)
            {
                return NotFound();
            }

            Transaction = transaction;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var transaction = await _context.Transactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.Id == Transaction.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            transaction.ReturnDate = DateTime.Today;
            
            // Calculate fine if overdue
            if (transaction.ReturnDate > transaction.DueDate)
            {
                var overdueDays = (transaction.ReturnDate.Value - transaction.DueDate).Days;
                transaction.Fine = overdueDays * 5; // $5 per day fine
            }

            // Update book availability
            if (transaction.Book != null)
            {
                transaction.Book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book returned successfully!";
            return RedirectToPage("./Index");
        }
    }
}