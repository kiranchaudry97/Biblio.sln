// Doel: ViewModel voor ledenbeheer; laden, zoeken, CRUD en soft-delete met validatie.
// Beschrijving: Beheert Items/Selected/Edit/Search, valideert invoer, checkt e-mail duplicaten en gebruikt EF Core async-ops.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Biblio.Dekstop.ViewModels
{
    public class MembersViewModel
    {
        private readonly BiblioDbContext _db;

        public ObservableCollection<Member> Items { get; } = new();
        public Member? Selected { get; set; }
        public Member Edit { get; set; } = new();
        public string? Search { get; set; }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand NewCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        public MembersViewModel(BiblioDbContext db)
        {
            _db = db;
            SearchCommand = new RelayCommand(async () => await LoadAsync());
            NewCommand = new RelayCommand(New);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            Items.Clear();
            var q = _db.Members.AsQueryable().Where(m => !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var term = $"%{Search}%";
                q = q.Where(m => EF.Functions.Like(m.FirstName, term) ||
                                 EF.Functions.Like(m.LastName, term) ||
                                 EF.Functions.Like(m.Email, term));
            }

            foreach (var m in await q.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToListAsync())
                Items.Add(m);
        }

        private void New() { Selected = null; Edit = new Member(); }

        private static bool IsValidEmail(string email)
        {
            try { _ = new MailAddress(email); return true; } catch { return false; }
        }

        private async Task SaveAsync()
        {
            try
            {
                Edit.FirstName = (Edit.FirstName ?? string.Empty).Trim();
                Edit.LastName = (Edit.LastName ?? string.Empty).Trim();
                Edit.Email = (Edit.Email ?? string.Empty).Trim();
                Edit.Phone = (Edit.Phone ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(Edit.FirstName) || Edit.FirstName.Length >100)
                { MessageBox.Show("Voornaam is verplicht (max100).", "Validatie"); return; }
                if (string.IsNullOrWhiteSpace(Edit.LastName) || Edit.LastName.Length >100)
                { MessageBox.Show("Achternaam is verplicht (max100).", "Validatie"); return; }
                if (string.IsNullOrWhiteSpace(Edit.Email) || Edit.Email.Length >256 || !IsValidEmail(Edit.Email))
                { MessageBox.Show("E-mail is verplicht en moet geldig zijn (max256).", "Validatie"); return; }
                if (!string.IsNullOrWhiteSpace(Edit.Phone) && Edit.Phone.Length >64)
                { MessageBox.Show("Telefoon is te lang.", "Validatie"); return; }
                if (!string.IsNullOrWhiteSpace(Edit.Address) && Edit.Address.Length >300)
                { MessageBox.Show("Adres is te lang (max300).", "Validatie"); return; }

                // Duplicate-check op e-mail (soft-deleted uitsluiten)
                bool exists = await _db.Members.AnyAsync(m => !m.IsDeleted && m.Email == Edit.Email && m.Id != Edit.Id);
                if (exists)
                {
                    MessageBox.Show("E-mail is al in gebruik.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Edit.Id ==0) _db.Members.Add(Edit); else _db.Members.Update(Edit);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number ==2601 || sql.Number ==2627))
                {
                    MessageBox.Show("E-mail is al in gebruik.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await LoadAsync();
                Selected = Items.FirstOrDefault(m => m.Id == Edit.Id);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Fout"); }
        }

        private async Task DeleteAsync()
        {
            if (Selected is null) return;
            if (MessageBox.Show($"Verwijder {Selected.FirstName} {Selected.LastName}?", "Bevestigen",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Selected.IsDeleted = true;
            Selected.DeletedAt = DateTime.UtcNow;
            _db.Members.Update(Selected);
            await _db.SaveChangesAsync();
            await LoadAsync();
            New();
        }
    }
}
