using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Data;
using SimpleLibrary.Models;

namespace SimpleLibrary.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveTransactions { get; set; }
    public int OverdueTransactions { get; set; }

    public async Task OnGetAsync()
    {
        TotalBooks = await _context.Books.CountAsync();
        AvailableBooks = await _context.Books.CountAsync(b => b.IsAvailable);
        TotalMembers = await _context.Members.CountAsync();
        ActiveTransactions = await _context.Transactions.CountAsync(t => t.ReturnDate == null);
        OverdueTransactions = await _context.Transactions
            .CountAsync(t => t.ReturnDate == null && t.DueDate < DateTime.Today);
    }
}
