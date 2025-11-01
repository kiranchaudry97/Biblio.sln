# CHECKLIST - Biblio solution

Korte status en instructies voor oplevering en beoordeling.

## Projectoverzicht
- Solution bevat2 projecten:
 - `Biblio.Models` (model library, EF Core, Identity)
 - `Biblio.Dekstop` (WPF .NET9 desktop client)
- API project aanwezig in workspace: `Biblio.Api` (voor referentie / integratie)

## Vereisten (rubric) — status
- [.✔] .NET9 WPF-applicatie (`Biblio.Dekstop` target net9.0-windows)
- [.✔] Solution met2 projecten (Model + Desktop)
- [.✔] Publieke GitHub repository beschikbaar

### Database / EF Core
- [.✔] Minstens3 relationele tabellen: `Books`, `Members`, `Categories`, `Loans`
- [.✔] `BiblioDbContext` afgeleid van `IdentityDbContext`
- [.✔] Connection via config (gebruik User Secrets / env voor secrets)
- [.✔] Migraties aanwezig (folder `Biblio.Models/Migrations`)
- [.✔] Seeders + dummy data aanwezig (`SeedData`)
- [.✔] Soft-delete op modellen (`BaseEntity.IsDeleted`, `DeletedAt`)
- [.✔] Globale query filter toegevoegd in `BiblioDbContext`

### Identity
- [.✔] Eigen `AppUser` met extra property (`FullName`)
- [.✔] Registratie, aanmelding en afmelding aanwezig (windows)
- [.✔] Minstens2 rollen (`Admin`, `Medewerker`) en seeded
- [.✔] Rolwijziging door admin mogelijk (`AdminUsersWindow`)
- [.✔] Menu zichtbaarheid op basis van rollen (SecurityViewModel)

### UI / MVVM
- [.✔] CRUD ViewModels voor belangrijkste modellen (Books/Members/Loans/Categories)
- [.✔] Minstens één selectieveld voor meerdere modellen (categorie/author)
- [.✔] Meerdere container types gebruikt (Grid, StackPanel, TabControl)
- [.✔] Menu/tab-structuur aanwezig
- [.✔] Popup windows aanwezig (Register, Profile, AdminUsers, AdminRestore)
- [.✔] Styles en thema’s aanwezig (Theme.Light/Theme.Dark)
- [.✔] Binding en MVVM gebruikt door de applicatie
- [.✔] Eén custom control aanwezig (`LabeledTextBox`)

### C# / LINQ / Foutafhandeling
- [.✔] Gebruik van method- en query-LINQ syntax (BooksViewModel)
- [.✔] Lambda-expressies gebruikt
- [.✔] Try/Catch en gebruikers‑feedback (MessageBox) aanwezig
- [.✔] App start foutloos (build + run geslaagd lokaal)

### Bonus
- [.✔] API consumer + secure token storage (DPAPI) in `Biblio.Dekstop`
- [.✔] Error handling middleware (ProblemDetails) in `Biblio.Api`

## Resterende aanbevolen acties (aan te tonen bij beoordeling)
1. Handmatige smoke-test (essentieel):
 - Migraties toepassen:
 - `dotnet ef database update --project Biblio.Models --startup-project Biblio.Dekstop`
 - Start desktop:
 - `dotnet run --project Biblio.Dekstop`
 - Testscenario:
 - Login als admin (gebruik seeded credentials uit `SeedOptions` of default `admin@biblio.local / Admin!23456`).
 - CRUD boeken/leden/uitleningen; verwijder (soft-delete) en herstel via `AdminRestoreWindow`.
 - Wijzig rollen in `AdminUsersWindow` en verifieer UI wijzigingen.
 - Controleer dat API‑aanroepen Authorization header gebruiken (wanneer API draait).

2. Documentatie:
 - Voeg screenshots en korte run‑instructies toe in README of `CHECKLIST.md`.
 - Zorg dat `Jwt:Key` niet in repository staat; gebruik `dotnet user-secrets` of env var `JWT__KEY`.

3. Tests & CI (aanbevolen):
 - Voeg unit tests toe (xUnit) voor seed, DbContext en minstens één ViewModel.
 - Maak GitHub Actions workflow voor build + tests.

4. Security / productie:
 - Gebruik Key Vault / secure env vars voor JWT in productie.
 - Bescherm Swagger UI in productie.

## Commando's voor beoordeling / reproduceerbaarheid
- Build solution:
 - `dotnet build`
- Apply migrations:
 - `dotnet ef database update --project Biblio.Models --startup-project Biblio.Dekstop`
- Run API (optioneel):
 - `dotnet run --project Biblio.Api`
- Run desktop client:
 - `dotnet run --project Biblio.Dekstop`

## Belangrijke bestanden (snel overzicht)
- Models/DbContext/Migrations: `Biblio.Models/*`
- Desktop (Views/ViewModels/Services): `Biblio.Dekstop/*`
- API (controllers, DTOs, middleware): `Biblio.Api/*`
- Scripts: `Biblio.Api/scripts/set-jwt-secret.ps1`

---

