// Bestand: Biblio.Dekstop/Views/AdminUsersWindow.xaml.cs
// Doel: Beheer van gebruikers en rollen; laden lijst, aanmaken nieuwe gebruiker en toewijzen Admin/Medewerker rollen via Identity.
// zie commit bericht 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Biblio.Dekstop.Views
{
 public partial class AdminUsersWindow : Window, INotifyPropertyChanged
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
 if (_selectedUser == value) return;
 _selectedUser = value;
 OnPropertyChanged(nameof(SelectedUser));
 _ = LoadSelectedUserRolesAsync();
 }
 }

 private bool _isAdmin;
 public bool IsAdmin
 {
 get => _isAdmin;
 set { if (_isAdmin == value) return; _isAdmin = value; OnPropertyChanged(nameof(IsAdmin)); }
 }

 private bool _isStaff;
 public bool IsStaff
 {
 get => _isStaff;
 set { if (_isStaff == value) return; _isStaff = value; OnPropertyChanged(nameof(IsStaff)); }
 }

 private string? _newEmail;
 public string? NewEmail
 {
 get => _newEmail;
 set { _newEmail = value; OnPropertyChanged(nameof(NewEmail)); }
 }
 private string? _newPassword;
 public string? NewPassword
 {
 get => _newPassword;
 set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); }
 }
 private string? _newFullName;
 public string? NewFullName
 {
 get => _newFullName;
 set { _newFullName = value; OnPropertyChanged(nameof(NewFullName)); }
 }

 private bool _newIsAdmin;
 public bool NewIsAdmin
 {
 get => _newIsAdmin;
 set { _newIsAdmin = value; OnPropertyChanged(nameof(NewIsAdmin)); }
 }

 private bool _newIsStaff = true;
 public bool NewIsStaff
 {
 get => _newIsStaff;
 set { _newIsStaff = value; OnPropertyChanged(nameof(NewIsStaff)); }
 }

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

 // Admin role
 if (IsAdmin && !await _userManager.IsInRoleAsync(SelectedUser, "Admin"))
 {
 var res = await _userManager.AddToRoleAsync(SelectedUser, "Admin");
 if (!res.Succeeded) throw new Exception(string.Join("; ", res.Errors.Select(x => x.Description)));
 }
 else if (!IsAdmin && await _userManager.IsInRoleAsync(SelectedUser, "Admin"))
 {
 var res = await _userManager.RemoveFromRoleAsync(SelectedUser, "Admin");
 if (!res.Succeeded) throw new Exception(string.Join("; ", res.Errors.Select(x => x.Description)));
 }

 // Staff role
 if (IsStaff && !await _userManager.IsInRoleAsync(SelectedUser, "Medewerker"))
 {
 var res = await _userManager.AddToRoleAsync(SelectedUser, "Medewerker");
 if (!res.Succeeded) throw new Exception(string.Join("; ", res.Errors.Select(x => x.Description)));
 }
 else if (!IsStaff && await _userManager.IsInRoleAsync(SelectedUser, "Medewerker"))
 {
 var res = await _userManager.RemoveFromRoleAsync(SelectedUser, "Medewerker");
 if (!res.Succeeded) throw new Exception(string.Join("; ", res.Errors.Select(x => x.Description)));
 }

 // refresh UI
 await LoadUsersAsync();
 await LoadSelectedUserRolesAsync();

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
 // Ensure roles exist before assigning
 if (!await _roleManager.RoleExistsAsync("Admin"))
 await _roleManager.CreateAsync(new IdentityRole("Admin"));
 if (!await _roleManager.RoleExistsAsync("Medewerker"))
 await _roleManager.CreateAsync(new IdentityRole("Medewerker"));

 var user = new AppUser { UserName = NewEmail.Trim(), Email = NewEmail.Trim(), FullName = NewFullName };
 var result = await _userManager.CreateAsync(user, NewPassword);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors.Select(e => e.Description)), "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }

 if (NewIsAdmin)
 {
 var r = await _userManager.AddToRoleAsync(user, "Admin");
 if (!r.Succeeded) MessageBox.Show(string.Join("\n", r.Errors.Select(x => x.Description)), "Fout Rollen", MessageBoxButton.OK, MessageBoxImage.Warning);
 }
 if (NewIsStaff)
 {
 var r = await _userManager.AddToRoleAsync(user, "Medewerker");
 if (!r.Succeeded) MessageBox.Show(string.Join("\n", r.Errors.Select(x => x.Description)), "Fout Rollen", MessageBoxButton.OK, MessageBoxImage.Warning);
 }

 await LoadUsersAsync();
 SelectedUser = Users.FirstOrDefault(u => u.Email == user.Email);

 NewEmail = NewPassword = NewFullName = string.Empty;
 NewIsAdmin = false; NewIsStaff = true;
 OnPropertyChanged(nameof(NewEmail));
 OnPropertyChanged(nameof(NewPassword));
 OnPropertyChanged(nameof(NewFullName));
 OnPropertyChanged(nameof(NewIsAdmin));
 OnPropertyChanged(nameof(NewIsStaff));

 MessageBox.Show("Gebruiker aangemaakt.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 }
 catch (Exception ex)
 {
 MessageBox.Show($"Aanmaken mislukt: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 }
 }

 private void OnOpenRestore(object sender, RoutedEventArgs e)
 {
 var wnd = App.AppHost.Services.GetRequiredService<AdminRestoreWindow>();
 wnd.Owner = this;
 wnd.ShowDialog();
 }

 public event PropertyChangedEventHandler? PropertyChanged;
 private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
 }
}
