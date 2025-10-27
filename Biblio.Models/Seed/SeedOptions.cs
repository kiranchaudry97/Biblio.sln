// Doel: Configuratieopties voor seeding van admin gebruiker (e-mail, naam, wachtwoord).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblio.Models.Seed
{
    public class SeedOptions
    {
        public string? AdminPassword { get; set; }
        public string AdminEmail { get; set; } = "admin@biblio.local";
        public string AdminFullName { get; set; } = "Beheerder";
    }
}
