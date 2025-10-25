using Biblio.Models.Data;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Biblio.Models.Seed;


public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BiblioDbContext>();
        await ctx.Database.MigrateAsync();


        // Rollen
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Medewerker" })
        {
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));
        }


        // Admin gebruiker
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        const string adminEmail = "admin@biblio.local";
        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser { UserName = adminEmail, Email = adminEmail, FullName = "Beheerder" };
            await userMgr.CreateAsync(admin, "Admin!23456"); // alleen voor demo; vervang via secrets
            await userMgr.AddToRoleAsync(admin, "Admin");
        }


        // Categorieën seed
        if (!await ctx.Categories.AnyAsync())
        {
            ctx.Categories.AddRange(
            new Category { Name = "Roman" },
            new Category { Name = "Jeugd" },
            new Category { Name = "Thriller" },
            new Category { Name = "Wetenschap" }
            );
            await ctx.SaveChangesAsync();
        }


        // Dummy Book/Member voor testen
        if (!await ctx.Books.AnyAsync())
        {
            var catId = await ctx.Categories.Where(c => c.Name == "Roman").Select(c => c.Id).FirstAsync();
            ctx.Books.Add(new Book { Title = "1984", Author = "George Orwell", Isbn = "9780451524935", CategoryId = catId });
        }
        if (!await ctx.Members.AnyAsync())
        {
            ctx.Members.Add(new Member { FirstName = "Jan", LastName = "Peeters", Email = "jan.peeters@example.com" });
        }
        await ctx.SaveChangesAsync();
    }
}