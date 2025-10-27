// Bestand: Biblio.Dekstop/Views/ProfileWindow.xaml.cs
// Doel: Code-behind voor profielbewerking; laadt huidige gegevens en valideert/opslaat wijzigingen via UserManager.
// Wijzigingen:
using System;
using System.Linq;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblio.Dekstop.Views
{
 public partial class ProfileWindow : Window
 {
 private readonly UserManager<AppUser> _userManager;
 private readonly AppUser _user;

 public ProfileWindow(UserManager<AppUser> userManager, AppUser user)
 {
 InitializeComponent();
 _userManager = userManager;
 _user = user;
 Loaded += (_, __) => LoadUi();
 }

 private void LoadUi()
 {
 FullNameBox.Text = _user.FullName ?? string.Empty;
 EmailBox.Text = _user.Email ?? string.Empty;
 PhoneBox.Text = _user.PhoneNumber ?? string.Empty;
 }

 private async void OnSave(object sender, RoutedEventArgs e)
 {
 var fullName = (FullNameBox.Text ?? string.Empty).Trim();
 var email = (EmailBox.Text ?? string.Empty).Trim();
 var phone = (PhoneBox.Text ?? string.Empty).Trim();

 if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
 { MessageBox.Show("Geef een geldig e-mailadres op.", "Validatie"); return; }
 if (fullName.Length >200)
 { MessageBox.Show("Volledige naam is te lang.", "Validatie"); return; }
 if (phone.Length >64)
 { MessageBox.Show("Telefoon is te lang.", "Validatie"); return; }

 _user.FullName = fullName;
 _user.Email = email;
 _user.UserName = email;
 _user.PhoneNumber = phone;

 var result = await _userManager.UpdateAsync(_user);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors.Select(e => e.Description)), "Fout");
 return;
 }
 MessageBox.Show("Profiel opgeslagen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 DialogResult = true;
 Close();
 }
 }
}
