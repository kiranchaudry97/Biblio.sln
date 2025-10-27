// Bestand: Biblio.Dekstop/Views/AdminUsersWindow.xaml.cs
// Doel: Beheer van gebruikers en rollen; laden lijst, aanmaken nieuwe gebruiker en toewijzen Admin/Medewerker rollen via Identity.
// Wijzigingen:
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Biblio.Dekstop.Views
{
 public partial class AdminUsersWindow : Window
 {
 private readonly UserManager<AppUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;

 public ObservableCollection<AppUser> Users { get; } = new();

 private AppUser? _selectedUser;
 public AppUser? SelectedUser
 {
 get => _selectedUser;
 set
 {
 _selectedUser = value;
 _ = LoadSelectedUserRolesAsync();
 }
 }

 public bool IsAdmin { get; set; }
 public bool IsStaff { get; set; }

 public string? NewEmail { get; set; }
 public string? NewPassword { get; set; }
 public string? NewFullName { get; set; }
 public bool NewIsAdmin { get; set; }
 public bool NewIsStaff { get; set; } = true;

 public AdminUsersWindow(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 InitializeComponent();
 _userManager = userManager;
 _roleManager = roleManager;
 DataContext = this;
 Loaded += async (_, __) => await LoadUsersAsync();
 }

 private async Task LoadUsersAsync()
 {
 Users.Clear();
 foreach (var u in await _userManager.Users.ToListAsync())
 Users.Add(u);
 }

 private async Task LoadSelectedUserRolesAsync()
 {
 if (SelectedUser is null) { IsAdmin = IsStaff = false; return; }
 var roles = await _userManager.GetRolesAsync(SelectedUser);
 IsAdmin = roles.Contains("Admin");
 IsStaff = roles.Contains("Medewerker");
 }

 private async void OnSaveRoles(object sender, RoutedEventArgs e)
 {
 if (SelectedUser is null) return;
 try
 {
 if (!await _roleManager.RoleExistsAsync("Admin"))
 await _roleManager.CreateAsync(new IdentityRole("Admin"));
 if (!await _roleManager.RoleExistsAsync("Medewerker"))
 await _roleManager.CreateAsync(new IdentityRole("Medewerker"));

 if (IsAdmin && !await _userManager.IsInRoleAsync(SelectedUser, "Admin"))
 await _userManager.AddToRoleAsync(SelectedUser, "Admin");
 else if (!IsAdmin && await _userManager.IsInRoleAsync(SelectedUser, "Admin"))
 await _userManager.RemoveFromRoleAsync(SelectedUser, "Admin");

 if (IsStaff && !await _userManager.IsInRoleAsync(SelectedUser, "Medewerker"))
 await _userManager.AddToRoleAsync(SelectedUser, "Medewerker");
 else if (!IsStaff && await _userManager.IsInRoleAsync(SelectedUser, "Medewerker"))
 await _userManager.RemoveFromRoleAsync(SelectedUser, "Medewerker");

 MessageBox.Show("Rollen opgeslagen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 }
 catch (Exception ex)
 {
 MessageBox.Show($"Opslaan mislukt: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 }
 }

 private async void OnCreateUser(object sender, RoutedEventArgs e)
 {
 // Lees wachtwoord uit PasswordBox in de XAML
 NewPassword = PwdBox.Password;
 if (string.IsNullOrWhiteSpace(NewEmail) || string.IsNullOrWhiteSpace(NewPassword))
 {
 MessageBox.Show("E-mail en wachtwoord zijn verplicht.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }

 try
 {
 var user = new AppUser { UserName = NewEmail.Trim(), Email = NewEmail.Trim(), FullName = NewFullName };
 var result = await _userManager.CreateAsync(user, NewPassword);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors.Select(e => e.Description)), "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }

 if (NewIsAdmin)
 await _userManager.AddToRoleAsync(user, "Admin");
 if (NewIsStaff)
 await _userManager.AddToRoleAsync(user, "Medewerker");

 NewEmail = NewPassword = NewFullName = string.Empty;
 NewIsAdmin = false; NewIsStaff = true;
 await LoadUsersAsync();
 MessageBox.Show("Gebruiker aangemaakt.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 }
 catch (Exception ex)
 {
 MessageBox.Show($"Aanmaken mislukt: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 }
 }
 }
}
