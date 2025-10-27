// Doel: Code-behind voor wachtwoord reset dialoog; valideert input en gebruikt UserManager voor reset.
// Beschrijving: Valideert en verwerkt invoer voor wachtwoordwijziging met ResetPasswordAsync via een gegenereerde token.
using System;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblio.Dekstop.Views
{
 public partial class ResetPasswordWindow : Window
 {
 private readonly UserManager<AppUser> _userManager;
 private readonly AppUser _user;

 public ResetPasswordWindow(UserManager<AppUser> userManager, AppUser user)
 {
 InitializeComponent();
 _userManager = userManager;
 _user = user;
 }

 private async void OnSave(object sender, RoutedEventArgs e)
 {
 var newPwd = NewBox.Password;
 var confirm = ConfirmBox.Password;
 if (string.IsNullOrWhiteSpace(newPwd))
 {
 MessageBox.Show("Geef een nieuw wachtwoord in.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }
 if (newPwd != confirm)
 {
 MessageBox.Show("Bevestiging komt niet overeen.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }
 // Desktop: reset zonder oud wachtwoord via reset-token (beschikbaar in AddIdentityCore)
 var token = await _userManager.GeneratePasswordResetTokenAsync(_user);
 var result = await _userManager.ResetPasswordAsync(_user, token, newPwd);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors), "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }
 MessageBox.Show("Wachtwoord ingesteld.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 DialogResult = true;
 Close();
 }
 }
}
