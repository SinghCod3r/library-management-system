using LibraryManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Create roles if they don't exist
            string[] roles = new string[] { "Admin", "Member" };
            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user if it doesn't exist
            if (await userManager.FindByEmailAsync("admin@library.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@library.com",
                    Email = "admin@library.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Add sample books if none exist
            if (!context.Books.Any())
            {
                var books = new Book[]
                {
                    new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", Category = "Fiction", IsAvailable = true },
                    new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", Category = "Fiction", IsAvailable = true },
                    new Book { Title = "1984", Author = "George Orwell", ISBN = "9780451524935", Category = "Fiction", IsAvailable = true },
                    new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "9780547928227", Category = "Fiction", IsAvailable = true },
                    new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884", Category = "Technology", IsAvailable = true }
                };

                foreach (Book b in books)
                {
                    context.Books.Add(b);
                }
                context.SaveChanges();
            }

            // Add sample members if none exist
            if (!context.Members.Any())
            {
                var members = new Member[]
                {
                    new Member { Name = "John Doe", Email = "john@example.com", MembershipType = "Standard", ExpiryDate = DateTime.Now.AddYears(1) },
                    new Member { Name = "Jane Smith", Email = "jane@example.com", MembershipType = "Premium", ExpiryDate = DateTime.Now.AddYears(1) }
                };

                foreach (Member m in members)
                {
                    context.Members.Add(m);
                }
                context.SaveChanges();
            }
        }
    }
}