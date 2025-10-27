// Doel: Design-time factory zodat 'dotnet ef' migrations kan uitvoeren zonder WPF startup.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Biblio.Models.Data
{
 // Allows 'dotnet ef' to create the DbContext at design-time without the WPF startup.
 public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BiblioDbContext>
 {
 public BiblioDbContext CreateDbContext(string[] args)
 {
 var optionsBuilder = new DbContextOptionsBuilder<BiblioDbContext>();
 // Fallback local connection string – override via User Secrets/appsettings in runtime
 var conn = "Server=(localdb)\\MSSQLLocalDB;Database=BiblioDb;Trusted_Connection=True;MultipleActiveResultSets=true";
 optionsBuilder.UseSqlServer(conn);
 return new BiblioDbContext(optionsBuilder.Options);
 }
 }
}
