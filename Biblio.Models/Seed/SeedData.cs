// Doel: Seed routine voor database: rollen, admin gebruiker en basisdata (categorieën, boeken, leden).
using System;
using System.Linq;
using System.Threading.Tasks;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Biblio.Models.Seed
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<BiblioDbContext>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
            var opts = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;

            //1️⃣ Database aanmaken (indien niet bestaat)
            await db.Database.MigrateAsync();

            //2️⃣ Rollen aanmaken
            string[] rollen = { "Admin", "Medewerker" };
            foreach (var role in rollen)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));
            }

            //3️⃣ Admin-gebruiker aanmaken of wachtwoord forceren (zonder tokenproviders)
            var adminEmail = string.IsNullOrWhiteSpace(opts.AdminEmail) ? "admin@biblio.local" : opts.AdminEmail;
            var desiredPwd = string.IsNullOrWhiteSpace(opts.AdminPassword) ? "Admin!23456" : opts.AdminPassword;
            var admin = await userMgr.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = string.IsNullOrWhiteSpace(opts.AdminFullName) ? "Beheerder" : opts.AdminFullName,
                    EmailConfirmed = true
                };

                // Maak gebruiker aan met wachtwoord
                var create = await userMgr.CreateAsync(admin, desiredPwd);
                if (!create.Succeeded)
                    throw new Exception("Fout bij aanmaken admin: " + string.Join(", ", create.Errors.Select(e => e.Description)));
            }
            else if (!string.IsNullOrWhiteSpace(opts.AdminPassword))
            {
                // Forceer nieuw wachtwoord door hash te overschrijven (geen tokenproviders nodig)
                admin.PasswordHash = hasher.HashPassword(admin, desiredPwd);
                var upd = await userMgr.UpdateAsync(admin);
                if (!upd.Succeeded)
                    throw new Exception("Fout bij resetten admin-wachtwoord: " + string.Join(", ", upd.Errors.Select(e => e.Description)));
            }

            // Admin in Admin-rol zetten
            admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin != null && !await userMgr.IsInRoleAsync(admin, "Admin"))
            {
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            //4️⃣ Basisdata seeden
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { Name = "Roman" },
                    new Category { Name = "Jeugd" },
                    new Category { Name = "Thriller" },
                    new Category { Name = "Wetenschap" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Books.AnyAsync())
            {
                var roman = await db.Categories.FirstAsync(c => c.Name == "Roman");
                var jeugd = await db.Categories.FirstAsync(c => c.Name == "Jeugd");
                var thriller = await db.Categories.FirstAsync(c => c.Name == "Thriller");
                var wetenschap = await db.Categories.FirstAsync(c => c.Name == "Wetenschap");

                db.Books.AddRange(
                    // Roman
                    new Book { Title = "1984", Author = "George Orwell", Isbn = "9780451524935", CategoryId = roman.Id },
                    new Book { Title = "De Hobbit", Author = "J.R.R. Tolkien", Isbn = "9780547928227", CategoryId = roman.Id },
                    new Book { Title = "Pride and Prejudice", Author = "Jane Austen", Isbn = "9781503290563", CategoryId = roman.Id },
                    new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", Isbn = "9780061120084", CategoryId = roman.Id },
                    new Book { Title = "Brave New World", Author = "Aldous Huxley", Isbn = "9780060850524", CategoryId = roman.Id },

                    // Jeugd
                    new Book { Title = "Matilda", Author = "Roald Dahl", Isbn = "9780142410370", CategoryId = jeugd.Id },
                    new Book { Title = "Harry Potter en de Steen der Wijzen", Author = "J.K. Rowling", Isbn = "9781408855652", CategoryId = jeugd.Id },

                    // Thriller
                    new Book { Title = "The Girl with the Dragon Tattoo", Author = "Stieg Larsson", Isbn = "9780307454546", CategoryId = thriller.Id },
                    new Book { Title = "The Da Vinci Code", Author = "Dan Brown", Isbn = "9780307474278", CategoryId = thriller.Id },

                    // Wetenschap
                    new Book { Title = "A Brief History of Time", Author = "Stephen Hawking", Isbn = "9780553380163", CategoryId = wetenschap.Id },
                    new Book { Title = "The Selfish Gene", Author = "Richard Dawkins", Isbn = "9780192860927", CategoryId = wetenschap.Id }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Members.AnyAsync())
            {
                db.Members.AddRange(
                    new Member { FirstName = "Jan", LastName = "Peeters", Email = "jan.peeters@example.com" },
                    new Member { FirstName = "Sara", LastName = "De Smet", Email = "sara.desmet@example.com" }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
