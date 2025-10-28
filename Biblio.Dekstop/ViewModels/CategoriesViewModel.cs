// Doel: ViewModel voor categoriebeheer; laden, toevoegen, hernoemen en soft-delete.
// Beschrijving: Beheert lijst van categorieën, valideert invoer en gebruikt EF Core async-commando's voor CRUD.
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace Biblio.Dekstop.ViewModels
{
 public class CategoriesViewModel
 {
 private readonly BiblioDbContext _db;
 public ObservableCollection<Category> Items { get; } = new();
 public Category? Selected { get; set; }
 public string? NewName { get; set; }

 public IAsyncRelayCommand AddCommand { get; }
 public IAsyncRelayCommand RenameCommand { get; }
 public IAsyncRelayCommand DeleteCommand { get; }

 public CategoriesViewModel(BiblioDbContext db)
 {
 _db = db;
 AddCommand = new AsyncRelayCommand(AddAsync);
 RenameCommand = new AsyncRelayCommand(RenameAsync);
 DeleteCommand = new AsyncRelayCommand(DeleteAsync);
 _ = LoadAsync();
 }

 private async Task LoadAsync()
 {
 Items.Clear();
 // LINQ query-syntax voorbeeld (i.p.v. method-syntax)
 var query = from c in _db.Categories
 where !c.IsDeleted
 orderby c.Name
 select c;

 foreach (var c in await query.ToListAsync())
 Items.Add(c);
 }

 private async Task AddAsync()
 {
 if (string.IsNullOrWhiteSpace(NewName)) { MessageBox.Show("Geef een naam op."); return; }
 _db.Categories.Add(new Category { Name = NewName.Trim() });
 await _db.SaveChangesAsync();
 NewName = string.Empty;
 await LoadAsync();
 }

 private async Task RenameAsync()
 {
 if (Selected is null) return;
 if (string.IsNullOrWhiteSpace(NewName)) { MessageBox.Show("Geef een nieuwe naam op."); return; }
 Selected.Name = NewName.Trim();
 _db.Categories.Update(Selected);
 await _db.SaveChangesAsync();
 await LoadAsync();
 }

 private async Task DeleteAsync()
 {
 if (Selected is null) return;
 if (MessageBox.Show($"Verwijder categorie '{Selected.Name}'?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
 Selected.IsDeleted = true;
 Selected.DeletedAt = DateTime.UtcNow;
 _db.Categories.Update(Selected);
 await _db.SaveChangesAsync();
 await LoadAsync();
 }
 }
}
