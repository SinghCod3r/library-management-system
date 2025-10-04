using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleLibrary.Data;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Authorization;

namespace SimpleLibrary.Pages.Members
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Member Member { get; set; } = default!;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Member.JoinedOn = DateTime.Today;
            _context.Members.Add(Member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Member added successfully!";
            return RedirectToPage("./Index");
        }
    }
}