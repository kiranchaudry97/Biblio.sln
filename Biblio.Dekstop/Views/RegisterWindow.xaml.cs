// Doel: Registratievenster code-behind; maakt nieuwe gebruiker en (optioneel) Medewerker-rol aan via Identity.
// Beschrijving: Valideert invoer, creëert AppUser met wachtwoord en voegt rol toe. Stelt IsRegistered in en sluit.
using System;
using System.Linq;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblio.Dekstop.Views
{
 public partial class RegisterWindow : Window
 {
 private readonly UserManager<AppUser> _userManager;
 private readonly RoleManager<IdentityRole> _roleManager;

 public bool IsRegistered { get; private set; }

 public RegisterWindow(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
 {
 InitializeComponent();
 _userManager = userManager;
 _roleManager = roleManager;
 }

 private async void OnRegister(object sender, RoutedEventArgs e)
 {
 var fullName = FullNameBox.Text?.Trim();
 var email = EmailBox.Text?.Trim();
 var pwd = PasswordBox.Password;
 var staff = StaffCheck.IsChecked == true;

 if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
 {
 MessageBox.Show("E-mail en wachtwoord zijn verplicht.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }

 var user = new AppUser { UserName = email, Email = email, FullName = fullName };
 var result = await _userManager.CreateAsync(user, pwd);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors.Select(e => e.Description)), "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }

 if (staff)
 {
 if (!await _roleManager.RoleExistsAsync("Medewerker"))
 await _roleManager.CreateAsync(new IdentityRole("Medewerker"));
 await _userManager.AddToRoleAsync(user, "Medewerker");
 }

 IsRegistered = true;
 DialogResult = true;
 Close();
 }
 }
}
