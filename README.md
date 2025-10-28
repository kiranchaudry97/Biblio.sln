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
- Voorbeeld: veilig gebruiker aanmaken vanuit `AdminUsersWindow`  
- Foutafhandeling & logging  
- Submissie  

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
Het is haalbaar binnen ≈ 80 uur en uitbreidbaar naar web- of mobiele versies.

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

## 4. Technische samenvatting
- **Target framework:** `net9.0-windows` (WPF, .NET 9)  
- **Solution bestaat uit twee projecten:**
  - `Biblio.Models` — entiteiten, `BiblioDbContext : IdentityDbContext<AppUser,...>`, migraties en seeders  
  - `Biblio.Desktop` — WPF frontend (Views, ViewModels), DI/Host, services, styles en assets  
- **Identity:**
  - Custom user: `AppUser : IdentityUser` met extra property `FullName`
  - Rollen: `Admin`, `Medewerker` (aangemaakt door seeder)
- **DI / Host:**
  - `App.xaml.cs` bouwt een `Host` met `Microsoft.Extensions.Hosting` en registreert DbContext, Identity, services, views en viewmodels  
- **EF Core:**
  - `BiblioDbContext` bevat `DbSet<Book>`, `DbSet<Member>`, `DbSet<Category>`, `DbSet<Loan>`
  - Unieke indexes en filters (bv. slechts 1 actieve uitlening per boek) zijn in `OnModelCreating` gedefinieerd  
- **WPF UI:**
  - MVVM-structuur (Views + ViewModels). Bindingen worden systematisch gebruikt.  
  - Styles en thema’s in `Biblio.Desktop/Styles/`  
  - Custom control aanwezig: `Biblio.Desktop/Controls/LabeledTextBox.xaml`  
- **Security:**
  - Geen plain-text wachtwoorden in repo — gebruik **User Secrets** of environment variables.  

---

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

## 9. Voorbeeld: veilig gebruiker aanmaken in `AdminUsersWindow` (kort)
`PasswordBox` is niet bindable — lees het wachtwoord in code-behind en gebruik `UserManager.CreateAsync(user, password)`. Na aanmaken clear `PwdBox`. Minimaliseer tijd dat plaintext in geheugen aanwezig is.

(Volledige voorbeeldcode vind je in repository of vraag me om een commit-ready implementatie van `OnCreateUser`.)

---

## 10. Foutafhandeling & logging
- Globale exception handlers zijn aanwezig in `App.xaml.cs` (Dispatcher en AppDomain).  
- ViewModels en services gebruiken `try/catch` en tonen meldingen via `MessageBox`.  
- Logging configureer je via `Microsoft.Extensions.Logging` in de host.

---



