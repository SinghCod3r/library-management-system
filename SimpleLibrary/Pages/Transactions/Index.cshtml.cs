using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Data;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Authorization;

namespace SimpleLibrary.Pages.Transactions
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Transaction> Transactions { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Transactions = await _context.Transactions
                .Include(t => t.Book)
                .Include(t => t.Member)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }
    }
}