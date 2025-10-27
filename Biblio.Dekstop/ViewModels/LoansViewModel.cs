using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient; // voor SqlException (2601/2627)

namespace Biblio.Dekstop.ViewModels
{
    public class LoansViewModel
    {
        private readonly BiblioDbContext _db;

        public ObservableCollection<Loan> Items { get; } = new();
        public Loan? Selected { get; set; }

        public ObservableCollection<Member> Members { get; } = new();
        public Member? SelectedMember { get; set; }

        public ObservableCollection<Book> AvailableBooks { get; } = new();
        public Book? SelectedBook { get; set; }

        public bool IsOnlyOpen
        {
            get => _isOnlyOpen;
            set { _isOnlyOpen = value; _ = LoadLoansAsync(); }
        }
        private bool _isOnlyOpen;

        public IAsyncRelayCommand LendCommand { get; }
        public IAsyncRelayCommand ReturnCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        public LoansViewModel(BiblioDbContext db)
        {
            _db = db;
            LendCommand = new AsyncRelayCommand(LendAsync);
            ReturnCommand = new AsyncRelayCommand(ReturnAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await LoadMembersAsync();
            await LoadAvailableBooksAsync();
            await LoadLoansAsync();
        }

        private async Task LoadMembersAsync()
        {
            Members.Clear();
            var all = await _db.Members.Where(m => !m.IsDeleted)
                                       .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
                                       .ToListAsync();
            foreach (var m in all)
                Members.Add(m);
        }

        private async Task LoadAvailableBooksAsync()
        {
            AvailableBooks.Clear();

            // Boek is 'beschikbaar' als er geen openstaande loan is zonder ReturnedAt
            var loanedIds = await _db.Loans.Where(l => l.ReturnedAt == null)
                                           .Select(l => l.BookId)
                                           .ToListAsync();

            var books = await _db.Books.Where(b => !b.IsDeleted && !loanedIds.Contains(b.Id))
                                       .OrderBy(b => b.Title)
                                       .ToListAsync();

            foreach (var b in books) AvailableBooks.Add(b);
        }

        private async Task LoadLoansAsync()
        {
            Items.Clear();
            var q = _db.Loans
                        .Include(l => l.Book).Include(l => l.Member)
                        .AsQueryable();
            if (IsOnlyOpen)
                q = q.Where(l => l.ReturnedAt == null);
            var loans = await q.OrderByDescending(l => l.StartDate).ToListAsync();
            foreach (var l in loans) Items.Add(l);
        }

        private async Task LendAsync()
        {
            if (SelectedMember is null)
            { MessageBox.Show("Kies een lid."); return; }
            if (SelectedBook is null)
            { MessageBox.Show("Kies een boek."); return; }

            // Extra validatie: dubbele openstaande lening voor hetzelfde boek voorkomen
            bool alreadyOpen = await _db.Loans.AnyAsync(l => l.BookId == SelectedBook.Id && l.ReturnedAt == null);
            if (alreadyOpen)
            {
                MessageBox.Show("Boek niet beschikbaar", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadAvailableBooksAsync();
                return;
            }

            var loan = new Loan
            {
                MemberId = SelectedMember.Id,
                BookId = SelectedBook.Id,
                StartDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(21)
            };

            _db.Loans.Add(loan);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                (ex.InnerException is SqlException sql && (sql.Number ==2601 || sql.Number ==2627)) ||
                (ex.InnerException?.Message?.Contains("IX_Uitleningen_BoekId_Actief", StringComparison.OrdinalIgnoreCase) == true))
            {
                MessageBox.Show("Boek niet beschikbaar", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                _db.Entry(loan).State = EntityState.Detached;
                await LoadAvailableBooksAsync();
                await LoadLoansAsync();
                return;
            }

            await LoadAvailableBooksAsync();
            await LoadLoansAsync();
            MessageBox.Show("Uitlening geregistreerd.");
        }

        private async Task ReturnAsync()
        {
            if (Selected is null) return;
            if (Selected.ReturnedAt != null)
            { MessageBox.Show("Deze uitlening is al teruggebracht."); return; }

            Selected.ReturnedAt = DateTime.Today;
            _db.Loans.Update(Selected);
            await _db.SaveChangesAsync();

            await LoadAvailableBooksAsync();
            await LoadLoansAsync();
        }

        private async Task DeleteAsync()
        {
            if (Selected is null) return;
            if (MessageBox.Show("Verwijder deze uitlening?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _db.Loans.Remove(Selected);
            await _db.SaveChangesAsync();
            await LoadAvailableBooksAsync();
            await LoadLoansAsync();
        }
    }
}
