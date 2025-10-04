using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Data;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Authorization;

namespace SimpleLibrary.Pages.Members
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Member> Members { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Members = await _context.Members.ToListAsync();
        }
    }
}