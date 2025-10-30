// Doel: ViewModel voor boekenbeheer; laden, filteren en CRUD met validatie.
// Beschrijving: Beheert Items/Categories/Authors, ondersteunt zoeken en auteursfilter, ISBN-validatie/duplicate-check en soft-delete.
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using Biblio.Dekstop.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Biblio.Dekstop.ViewModels
{
    public class BooksViewModel
    {
        private readonly BiblioDbContext _db;
        private readonly IBookApiClient? _apiClient;

        // Data voor DataGrid en ComboBox
        public ObservableCollection<Book> Items { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<string> Authors { get; } = new();

        // Geselecteerd item in het grid
        private Book? _selected;
        public Book? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                if (value != null)
                {
                    // Kopie naar Edit zodat je niet rechtstreeks in EF-tracked entity wijzigt
                    Edit = new Book
                    {
                        Id = value.Id,
                        Title = value.Title,
                        Author = value.Author,
                        Isbn = value.Isbn,
                        CategoryId = value.CategoryId
                    };
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == value.CategoryId);
                }
            }
        }

        // Form-model
        public Book Edit { get; set; } = new();
        public Category? SelectedCategory { get; set; }
        public string? Search { get; set; }

        // Filter op auteur
        private string? _selectedAuthorFilter;
        public string? SelectedAuthorFilter
        {
            get => _selectedAuthorFilter;
            set { _selectedAuthorFilter = value; _ = LoadAsync(); }
        }

        // Commands voor XAML-bindings
        public IRelayCommand SearchCommand { get; }
        public IRelayCommand NewCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        public BooksViewModel(BiblioDbContext db, IBookApiClient? apiClient = null)
        {
            _db = db;
            _apiClient = apiClient;

            SearchCommand = new RelayCommand(async () => await LoadAsync());
            NewCommand = new RelayCommand(New);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await LoadCategoriesAsync();
            await LoadAuthorsAsync();
            await LoadAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var all = await _db.Categories
                               .OrderBy(c => c.Name)
                               .ToListAsync();
            foreach (var c in all) Categories.Add(c);
        }

        private async Task LoadAuthorsAsync()
        {
            Authors.Clear();
            // Optie voor geen filter
            Authors.Add("Alle auteurs");

            // Voorbeeld LINQ query-expression (query-syntax) — voldoet aan rubric-eis om beide stijlen te tonen
            var queryExpr = from b in _db.Books
                             where !b.IsDeleted && b.Author != null && b.Author != ""
                             select b.Author!;

            var names = await queryExpr
                               .Distinct()
                               .OrderBy(a => a)
                               .ToListAsync();

            // Oude method-syntax (alternatief) — behouden als referentie
            // var names = await _db.Books
            // .Where(b => !b.IsDeleted && b.Author != null && b.Author != "")
            // .Select(b => b.Author!)
            // .Distinct()
            // .OrderBy(a => a)
            // .ToListAsync();

            foreach (var n in names) Authors.Add(n);
            if (string.IsNullOrEmpty(SelectedAuthorFilter))
                SelectedAuthorFilter = Authors.FirstOrDefault();
        }

        public async Task LoadAsync()
        {
            Items.Clear();

            if (_apiClient != null)
            {
                try
                {
                    var apiBooks = await _apiClient.GetAllAsync();
                    foreach (var b in apiBooks)
                        Items.Add(b);
                    return;
                }
                catch (Exception ex)
                {
                    // fallback to DB if API fails
                    MessageBox.Show($"API call failed: {ex.Message}", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            var q = _db.Books
                       .Include(b => b.Category)
                       .Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var term = $"%{Search}%";
                q = q.Where(b =>
                    EF.Functions.Like(b.Title, term) ||
                    EF.Functions.Like(b.Author, term) ||
                    EF.Functions.Like(b.Isbn, term));
            }

            if (!string.IsNullOrWhiteSpace(SelectedAuthorFilter) && SelectedAuthorFilter != "Alle auteurs")
            {
                q = q.Where(b => b.Author == SelectedAuthorFilter);
            }

            foreach (var b in await q.OrderBy(b => b.Title).ToListAsync())
                Items.Add(b);
        }

        private void New()
        {
            Selected = null;
            SelectedCategory = null;
            Edit = new Book();
        }

        // ISBN helpers
        private static string NormalizeIsbn(string isbn)
            => new string((isbn ?? string.Empty).Where(ch => ch != '-' && ch != ' ').ToArray());

        private static bool IsValidIsbn(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var isbn = NormalizeIsbn(raw);
            if (isbn.Length == 10) return IsValidIsbn10(isbn);
            if (isbn.Length == 13) return IsValidIsbn13(isbn);
            return false;
        }

        private static bool IsValidIsbn10(string isbn10)
        {
            //9 digits + check (0-9 or X)
            if (isbn10.Length != 10) return false;
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn10[i])) return false;
                sum += (isbn10[i] - '0') * (10 - i);
            }
            int check;
            char last = isbn10[9];
            if (last == 'X' || last == 'x') check = 10;
            else if (char.IsDigit(last)) check = last - '0';
            else return false;
            sum += check;
            return sum % 11 == 0;
        }

        private static bool IsValidIsbn13(string isbn13)
        {
            if (isbn13.Length != 13 || !isbn13.All(char.IsDigit)) return false;
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int d = isbn13[i] - '0';
                sum += (i % 2 == 0) ? d : d * 3;
            }
            int check = (10 - (sum % 10)) % 10;
            return check == (isbn13[12] - '0');
        }

        private async Task SaveAsync()
        {
            try
            {
                if (SelectedCategory != null)
                    Edit.CategoryId = SelectedCategory.Id;

                // Normaliseren
                Edit.Title = (Edit.Title ?? string.Empty).Trim();
                Edit.Author = (Edit.Author ?? string.Empty).Trim();
                Edit.Isbn = (Edit.Isbn ?? string.Empty).Trim();

                // Validatie
                if (string.IsNullOrWhiteSpace(Edit.Title) || Edit.Title.Length > 200)
                {
                    MessageBox.Show("Titel is verplicht (max200).", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(Edit.Author) || Edit.Author.Length > 200)
                {
                    MessageBox.Show("Auteur is verplicht (max200).", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Edit.CategoryId == 0)
                {
                    MessageBox.Show("Kies een categorie.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ISBN: als ingevuld -> normaliseren en valideren (ISBN-10 of ISBN-13)
                if (!string.IsNullOrWhiteSpace(Edit.Isbn))
                {
                    var norm = NormalizeIsbn(Edit.Isbn);
                    if (!IsValidIsbn(norm))
                    {
                        MessageBox.Show("ISBN ongeldig (ISBN-10 of ISBN-13).", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    Edit.Isbn = norm;

                    // Duplicate-check tegen genormaliseerde waarden in DB
                    bool isbnBestaat = await _db.Books
                                                .Where(b => b.Id != Edit.Id && !b.IsDeleted)
                                                .AnyAsync(b => (b.Isbn ?? "").Replace("-", string.Empty).Replace(" ", string.Empty) == norm);
                    if (isbnBestaat)
                    {
                        MessageBox.Show("ISBN is al in gebruik.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (_apiClient != null)
                {
                    // Use API for create/update
                    if (Edit.Id == 0)
                    {
                        var created = await _apiClient.CreateAsync(Edit);
                        // refresh list
                        await LoadAuthorsAsync();
                        await LoadAsync();
                        Selected = Items.FirstOrDefault(b => b.Id == created.Id);
                    }
                    else
                    {
                        await _apiClient.UpdateAsync(Edit.Id, Edit);
                        await LoadAuthorsAsync();
                        await LoadAsync();
                        Selected = Items.FirstOrDefault(b => b.Id == Edit.Id);
                    }

                    MessageBox.Show("Boek opgeslagen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (Edit.Id == 0)
                    _db.Books.Add(Edit);
                else
                    _db.Books.Update(Edit);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (
                    (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627)) ||
                    (ex.InnerException?.Message?.Contains("IX_Boeken_ISBN", StringComparison.OrdinalIgnoreCase) == true))
                {
                    MessageBox.Show("ISBN is al in gebruik.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await LoadAuthorsAsync();
                await LoadAsync();

                // selecteer het zojuist opgeslagen item
                Selected = Items.FirstOrDefault(b => b.Id == Edit.Id);
                MessageBox.Show("Boek opgeslagen.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (Selected is null)
            {
                MessageBox.Show("Geen boek geselecteerd.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Verwijder '{Selected.Title}'?", "Bevestigen",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                if (_apiClient != null)
                {
                    await _apiClient.DeleteAsync(Selected.Id);
                    await LoadAuthorsAsync();
                    await LoadAsync();
                    New();
                    return;
                }

                // Soft delete
                Selected.IsDeleted = true;
                Selected.DeletedAt = DateTime.UtcNow;

                _db.Books.Update(Selected);
                await _db.SaveChangesAsync();
                await LoadAuthorsAsync();
                await LoadAsync();
                New(); // formulier leegmaken
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij verwijderen: {ex.Message}", "Fout",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
