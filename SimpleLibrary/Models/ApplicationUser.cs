using Microsoft.AspNetCore.Identity;
using SimpleLibrary.Models;

namespace SimpleLibrary.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? MemberId { get; set; }
        public Member? Member { get; set; }
    }
}