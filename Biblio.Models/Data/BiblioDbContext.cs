using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Biblio.Models.Data;


public class BiblioDbContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Loan> Loans => Set<Loan>();


    public BiblioDbContext(DbContextOptions<BiblioDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);


        // Relaties
        b.Entity<Book>()
        .HasOne(x => x.Category)
        .WithMany(c => c.Books)
        .HasForeignKey(x => x.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);


        b.Entity<Loan>()
        .HasOne(l => l.Book)
        .WithMany(bk => bk.Loans)
        .HasForeignKey(l => l.BookId);


        b.Entity<Loan>()
        .HasOne(l => l.Member)
        .WithMany(m => m.Loans)
        .HasForeignKey(l => l.MemberId);


        // Soft‑delete global filters
        b.Entity<Book>().HasQueryFilter(e => !e.IsDeleted);
        b.Entity<Member>().HasQueryFilter(e => !e.IsDeleted);
        b.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        b.Entity<Loan>().HasQueryFilter(e => !e.IsDeleted);
    }
}