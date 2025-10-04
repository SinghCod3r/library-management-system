using Microsoft.EntityFrameworkCore;
using SimpleLibrary.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SimpleLibrary.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(a => a.Member)
            .WithOne(m => m.ApplicationUser)
            .HasForeignKey<ApplicationUser>(a => a.MemberId);

        builder.Entity<Member>()
            .HasOne(m => m.ApplicationUser)
            .WithOne(a => a.Member)
            .HasForeignKey<Member>(m => m.ApplicationUserId);
    }
}