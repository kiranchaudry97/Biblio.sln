# Biblio — Bibliotheekbeheer (WPF, .NET 9)

**Initiatiefnemer:** Chaud-Ry Kiran Jamil  
**Projectnaam:** Biblio — Bibliotheekbeheer in WPF (.NET 9)

---

## Inhoud
- Projectdoel en korte omschrijving  
- Doel & motivatie  
- Technische samenvatting en vereisten  
- Datamodel (tabellen en relaties)  
- Project- en mappenstructuur  
- Screenshots (voorbeeldafbeeldingen)  
- Lokaal uitvoeren: stappen en commando’s  
- Identity, security en seed-details   
- Foutafhandeling & logging
- Optioneel: API & JWT-key beheer



---

## 1. Korte omschrijving
`Biblio` is een WPF-desktopapplicatie (.NET 9) voor het beheren van boeken, leden en uitleningen binnen een fictieve **Bibliotheek Anderlecht**.

### Belangrijkste functionaliteit
- Medewerkers kunnen boeken en leden registreren en beheren.  
- Uitleningen en terugbrengingen registreren en opvolgen.  
- Rapporten genereren (late uitleningen, meest geleende boeken, enz.).  
- Rolgebaseerde toegang (`Admin`, `Medewerker`) met UI-autorisatie.  
- Relationele database met tabellen: `Boeken`, `Leden`, `Uitleningen`, `Categorieën`.

---

## 2. Doel en motivatie
**Doel:** bibliotheken ondersteunen in hun dagelijkse werking door het uitlenen en beheren van boeken eenvoudig, overzichtelijk en efficiënt te maken.  
**Motivatie:** dit project combineert leerwaarde (MVVM, EF Core, Identity, security) met praktische toepasbaarheid.  
Het is haalbaar binnen ≈ 80 uur en uitbreidbaar naar web-versie.

---

## 3. Datamodel — tabellen & relaties
- **Boeken** (`Book`)
  - Velden: `BoekId` (PK), `Titel`, `Auteur`, `ISBN`, `CategoryId`, `IsDeleted`
  - Relatie: 1 `Book` → N `Loan`
- **Leden** (`Member`)
  - Velden: `LidId` (PK), `Voornaam`, `Naam`, `Tel`, `Email`, `Adres`, `IsDeleted`
  - Relatie: 1 `Member` → N `Loan`
- **Uitleningen** (`Loan`)
  - Velden: `UitleningId` (PK), `BoekId` (FK → Boeken.BoekId), `LidId` (FK → Leden.LidId), `StartDatum`, `EindDatum`, `IngeleverdOp` (nullable), `IsDeleted`
- **Categorieën** (`Category`)
  - Velden: `CategoryId` (PK), `Naam`, `Omschrijving`

ERD (in woorden):  
`Boeken` (1) ↔ (N) `Uitleningen` en `Leden` (1) ↔ (N) `Uitleningen`.

Soft-delete is aanwezig: alle modellen bevatten `IsDeleted` en `BiblioDbContext` definieert global query filters.

---

## 4. Technische samenvatting + Implentatie

- **Target framework:** `net9.0-windows` (WPF, .NET 9)  
- **Solution bestaat uit twee projecten:**
  - `Biblio.Models` — entiteiten, migraties, seeders, en `BiblioDbContext : IdentityDbContext<AppUser,...>`  
  - `Biblio.Dekstop` — WPF frontend (Views, ViewModels), DI/Host, services, styles en assets  
- **Identity:**  
  - Custom user: `AppUser : IdentityUser` met extra property `FullName` (`Biblio.Models/Entities/AppUser.cs`)  
  - Rollen: `Admin`, `Medewerker` (aangemaakt door seeder `SeedData`)  
- **DI / Host:**  
  - `App.xaml.cs` bouwt een `Host` (via `Microsoft.Extensions.Hosting`) en registreert `BiblioDbContext`, Identity, services, viewmodels en views.  
  - Identity is geregistreerd met `AddIdentityCore` + role stores (zie `App.xaml.cs`).  
- **EF Core:**  
  - `BiblioDbContext` bevat `DbSet<Book>`, `DbSet<Member>`, `DbSet<Category>`, `DbSet<Loan>`.  
  - `OnModelCreating` bevat NL-kolomnamen, unieke indexes en global query-filters voor soft-delete.  
  - Ook aanwezig: index/filter om maximaal één actieve uitlening per boek toe te laten.  
- **WPF UI:**  
  - MVVM-architectuur: Views in `Biblio.Dekstop/Views`, ViewModels in `Biblio.Dekstop/ViewModels`.  
  - Bindingen worden systematisch gebruikt.  
  - Styles/thema's in `Biblio.Dekstop/Styles/`.  
  - Custom control `Biblio.Dekstop/Controls/LabeledTextBox.xaml` aanwezig.  
- **Security:**  
  - Geen plain-text wachtwoorden in repository.  
  - Gebruik `dotnet user-secrets` of environment variables voor connection strings en seed wachtwoorden.  
  - Identity (`UserManager`) verzorgt hashing en opslag van wachtwoorden.  

---

### Voorbeeld — veilig gebruiker aanmaken in `AdminUsersWindow` (technische implementatie)

#### ⚙️ Wat er nodig is
- **UI:** `Biblio.Dekstop/Views/AdminUsersWindow.xaml` met o.a.:
  - `PasswordBox x:Name="PwdBox"`
  - Binding-velden: `NewEmail`, `NewFullName`, `NewIsAdmin`, `NewIsStaff`  
- **Code-behind:** `Biblio.Dekstop/Views/AdminUsersWindow.xaml.cs`  
  - Injectie via DI: `UserManager<AppUser>` en `RoleManager<IdentityRole>`  
- **Namespaces:**  
  `System.Runtime.InteropServices`, `System.Security`, `Microsoft.AspNetCore.Identity`,  
  `System.Linq`, `System.Windows`.

#### 🧱 Belangrijke principes
- `PasswordBox` is **niet bindable** — lees `PwdBox.SecurePassword` in code-behind.  
- Identity verwacht een `string` wachtwoord in `UserManager.CreateAsync(user, password)`, dus een korte conversie van `SecureString` → `string` is vereist.  
- Minimaliseer plaintext-exposure: wis het wachtwoord onmiddellijk met `PwdBox.Clear()`.  
- Laat **Identity** zelf de hashing uitvoeren en gebruik `RoleManager` voor rolbeheer.  

---

### 🧩 Voorbeeldcode — `AdminUsersWindow.xaml.cs`

> **Bestand:** `Biblio.Dekstop/Views/AdminUsersWindow.xaml.cs`

```csharp
// Vereiste usings:
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using Microsoft.AspNetCore.Identity;
using Biblio.Models.Entities;

namespace Biblio.Dekstop.Views
{
    public partial class AdminUsersWindow : Window
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersWindow(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            InitializeComponent();
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //  Hulpmethode: converteer tijdelijk SecureString → plaintext
        private static string? ToInsecureString(SecureString? secure)
        {
            if (secure == null) return null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secure);
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        //  Eventhandler: nieuwe gebruiker aanmaken
        private async void OnCreateUser(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = (NewEmail ?? string.Empty).Trim();
                var fullName = (NewFullName ?? string.Empty).Trim();

                // Tijdelijke conversie van SecureString naar plaintext
                var password = ToInsecureString(PwdBox.SecurePassword);

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Email en wachtwoord zijn verplicht.", "Validatie",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, password);

                // Clear plaintext exposure ASAP
                PwdBox.Clear();

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(Environment.NewLine, createResult.Errors.Select(x => x.Description));
                    MessageBox.Show($"Kon gebruiker niet aanmaken:\n{errors}", "Fout",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Zorg dat rollen bestaan en wijs ze toe
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!await _roleManager.RoleExistsAsync("Medewerker"))
                    await _roleManager.CreateAsync(new IdentityRole("Medewerker"));

                if (NewIsAdmin)
                    await _userManager.AddToRoleAsync(user, "Admin");
                if (NewIsStaff)
                    await _userManager.AddToRoleAsync(user, "Medewerker");

                await LoadUsersAsync(); // herlaad gebruikerslijst in UI

                // Reset UI velden
                NewEmail = NewFullName = null;
                NewIsAdmin = NewIsStaff = false;

                MessageBox.Show("Gebruiker succesvol aangemaakt.", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Onverwachte fout: {ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
```
## 5. Project- en mappenstructuur

```text
Biblio/                     ← Solution-root
│
├── Biblio.Models/           ← Data- en logica-laag (EF Core + Identity)
│   ├── Entities/            ← Klassen: Book, Member, Loan, Category, AppUser
│   ├── Data/
│   │   ├── BiblioDbContext.cs
│   │   ├── Configurations/  ← Fluent API-configuraties
│   │   └── Seed/            ← SeedData.cs, SeedOptions.cs
│   ├── Migrations/          ← EF Core migratiebestanden
│   ├── Services/            ← Business services / repositories
│   └── Biblio.Models.csproj
│
├── Biblio.Desktop/          ← WPF-laag (MVVM, UI, Views, ViewModels)
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml
│   ├── Views/
│   │   ├── BooksWindow.xaml
│   │   ├── MembersWindow.xaml
│   │   ├── AdminWindow.xaml
│   │   ├── ProfileWindow.xaml
│   │   └── LoansWindow.xaml
│   ├── ViewModels/
│   │   ├── BooksViewModel.cs
│   │   ├── MembersViewModel.cs
│   │   ├── AdminViewModel.cs
│   │   ├── ProfileViewModel.cs
│   │   └── LoansViewModel.cs
│   ├── Controls/
│   │   └── LabeledTextBox.xaml
│   ├── Styles/
│   │   ├── Colors.xaml
│   │   ├── Controls.xaml
│   │   └── Theme.xaml
│   ├── Assets/
│   │   └── screenshots/
│   └── Biblio.Desktop.csproj
│
└── Biblio.sln

```



---

## 6. Screenshots — placeholders
Plaats later screenshots


---

## 7. Lokaal uitvoeren — stappen (kopieer / plak)

1) Clone de repository
git clone https://github.com/kiranchaudry97/Biblio.git
 cd Biblio


2) (Optioneel) Pas connection string aan  
- In `appsettings.json` of via __User Secrets__ (aanbevolen). Voorbeeld `appsettings.json`:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BiblioDb;Trusted_Connection=True;"
}


- Voor user secrets (project `Biblio.Dekstop` heeft een UserSecretsId):
dotnet user-secrets init --project Biblio.Dekstop dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BiblioDb;Trusted_Connection=True;MultipleActiveResultSets=true" --project Biblio.Dekstop



3) Update database (migraties & seed)  
Let op: projectnaam in workspace is `Biblio.Dekstop` (niet `Biblio.Desktop`). Gebruik het volgende commando:
dotnet ef database update --project Biblio.Models --startup-project Biblio.Desktop



4) Start applicatie  
dotnet run --project Biblio.Desktop


Alternatief: open solution in Visual Studio 2026, zet `Biblio.Dekstop` als startup project, build via __Build__ → __Build Solution__ en run via __Debug__ → __Start Debugging__ of __Start Without Debugging__.

---

## 8. Identity, seeding & security
- Seeder (`Biblio.Models/Seed/SeedData.cs`) maakt rollen `Admin` en `Medewerker`, plus een admin-account en voorbeelddata.
- Seeder wordt aangeroepen bij startup en voert `db.Database.MigrateAsync()` uit.
- Gebruik __User Secrets__ of environment variables voor gevoelige data; bewaar geen plain-text wachtwoorden in de repo.

---



---

## 9. Foutafhandeling & logging
- Globale exception handlers zijn aanwezig in `App.xaml.cs` (Dispatcher en AppDomain).  
- ViewModels en services gebruiken `try/catch` en tonen meldingen via `MessageBox`.  
- Logging configureer je via `Microsoft.Extensions.Logging` in de host.

---



---

## 10. API & JWT-key beheer
- 10.1 Automatisch genereren van een sterke JWT key (lokaal)

Script: Biblio.Api/scripts/set-jwt-secret.ps1

Run vanuit repo root (PowerShell):
```text

powershell -ExecutionPolicy Bypass -File Biblio.Api\scripts\set-jwt-secret.ps1 -ProjectDir Biblio.Api
```

Het script initialeert user-secrets (indien nodig) en zet Jwt:Key in de geheime store.
Controleer:
```text
dotnet user-secrets list --project Biblio.Api
```

User-secrets zijn voor development, komen niet in source control.
---

### Alternatief: environment variable (CI/Production)

- Windows (session):
  
```text
$env:JWT__KEY = "<sterke-key>"
```

- Windows (permanent):
```text
setx JWT__KEY "<sterke-key>"
```

- Linux/macOS:
```text
export JWT__KEY="<sterke-key>"
```

### Migrations / database (API startup)
- Api run: 
  
```text
dotnet run --project Biblio.Api
```
### Migrations / database (API startup)
```text
"Desktop": {
  "AllowedOrigin": "http://localhost:5003"
}
```


