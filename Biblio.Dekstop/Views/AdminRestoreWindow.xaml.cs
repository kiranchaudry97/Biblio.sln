using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Entities;
using Biblio.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biblio.Dekstop.Views;

public partial class AdminRestoreWindow : Window
{
    private readonly BiblioDbContext _db;

    public AdminRestoreWindow(BiblioDbContext db)
    {
        InitializeComponent();
        _db = db;
        Loaded += async (_, __) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var deletedBooks = await _db.Books.IgnoreQueryFilters().Where(b => b.IsDeleted).OrderByDescending(b => b.DeletedAt).ToListAsync();
        BooksGrid.ItemsSource = deletedBooks;
        var deletedMembers = await _db.Members.IgnoreQueryFilters().Where(m => m.IsDeleted).OrderByDescending(m => m.DeletedAt).ToListAsync();
        MembersGrid.ItemsSource = deletedMembers;
        var deletedLoans = await _db.Loans.IgnoreQueryFilters().Include(l => l.Book).Include(l => l.Member).Where(l => l.IsDeleted).OrderByDescending(l => l.DeletedAt).ToListAsync();
        LoansGrid.ItemsSource = deletedLoans;
        var deletedCategories = await _db.Categories.IgnoreQueryFilters().Where(c => c.IsDeleted).OrderByDescending(c => c.DeletedAt).ToListAsync();
        CategoriesGrid.ItemsSource = deletedCategories;
    }

    private async void OnRefresh(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void OnRestoreBook(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItem is not Book b) { MessageBox.Show("Geen boek geselecteerd."); return; }
        b.IsDeleted = false; b.DeletedAt = null;
        _db.Books.Update(b);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }

    private async void OnRestoreMember(object sender, RoutedEventArgs e)
    {
        if (MembersGrid.SelectedItem is not Member m) { MessageBox.Show("Geen lid geselecteerd."); return; }
        m.IsDeleted = false; m.DeletedAt = null;
        _db.Members.Update(m);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }

    private async void OnRestoreLoan(object sender, RoutedEventArgs e)
    {
        if (LoansGrid.SelectedItem is not Loan l) { MessageBox.Show("Geen uitlening geselecteerd."); return; }
        l.IsDeleted = false; l.DeletedAt = null;
        _db.Loans.Update(l);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }

    private async void OnRestoreCategory(object sender, RoutedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not Category c) { MessageBox.Show("Geen categorie geselecteerd."); return; }
        c.IsDeleted = false; c.DeletedAt = null;
        _db.Categories.Update(c);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }
}