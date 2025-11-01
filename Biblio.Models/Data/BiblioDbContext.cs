// Doel: EF Core DbContext met ASP.NET Identity, entiteitstabellen, NL kolomnamen, relaties, filters en unieke indexes.
// Beschrijving: Configureert tabellen/kolommen, relaties, global query filters (soft-delete) en unieke indexen; integreert Identity.
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

 // Tabellen en kolomnamen (NL)
 b.Entity<Book>(e =>
 {
 e.ToTable("Boeken");
 e.Property(x => x.Id).HasColumnName("BoekId");
 e.Property(x => x.Title).HasColumnName("Titel");
 e.Property(x => x.Author).HasColumnName("Auteur");
 e.Property(x => x.Isbn).HasColumnName("ISBN");
 });

 b.Entity<Member>(e =>
 {
 e.ToTable("Leden");
 e.Property(x => x.Id).HasColumnName("LidId");
 e.Property(x => x.FirstName).HasColumnName("Voornaam");
 e.Property(x => x.LastName).HasColumnName("Naam");
 e.Property(x => x.Phone).HasColumnName("Tel");
 e.Property(x => x.Address).HasColumnName("Adres");
 });

 b.Entity<Loan>(e =>
 {
 e.ToTable("Uitleningen");
 e.Property(x => x.Id).HasColumnName("UitleningId");
 e.Property(x => x.BookId).HasColumnName("BoekId");
 e.Property(x => x.MemberId).HasColumnName("LidId");
 e.Property(x => x.StartDate).HasColumnName("StartDatum");
 e.Property(x => x.DueDate).HasColumnName("EindDatum");
 e.Property(x => x.ReturnedAt).HasColumnName("IngeleverdOp");

 // Max. één actieve uitlening per boek (IngeleverdOp is NULL)
 e.HasIndex(x => x.BookId)
 .HasFilter("([IngeleverdOp] IS NULL)")
 .HasDatabaseName("IX_Uitleningen_BoekId_Actief")
 .IsUnique();
 });

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

 // Unieke indexes voor data-integriteit
 b.Entity<Member>()
 .HasIndex(m => m.Email)
 .IsUnique()
 .HasFilter("([Email] IS NOT NULL AND [Email] <> '')");

 b.Entity<Book>()
 .HasIndex(bk => bk.Isbn)
 .IsUnique()
 .HasFilter("([Isbn] IS NOT NULL AND [Isbn] <> '')");
 }
}