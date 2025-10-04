using Microsoft.AspNetCore.Identity;

namespace SimpleLibrary.Models;

public class Member
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime JoinedOn { get; set; } = DateTime.Today;
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}