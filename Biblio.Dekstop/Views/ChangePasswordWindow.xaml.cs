using System;
using System.Windows;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblio.Dekstop.Views
{
 public partial class ChangePasswordWindow : Window
 {
 private readonly UserManager<AppUser> _userManager;
 private readonly AppUser _user;

 public ChangePasswordWindow(UserManager<AppUser> userManager, AppUser user)
 {
 InitializeComponent();
 _userManager = userManager;
 _user = user;
 }

 private async void OnChange(object sender, RoutedEventArgs e)
 {
 var oldPwd = OldBox.Password;
 var newPwd = NewBox.Password;
 var confirm = ConfirmBox.Password;
 if (string.IsNullOrWhiteSpace(oldPwd) || string.IsNullOrWhiteSpace(newPwd))
 {
 MessageBox.Show("Vul alle velden in.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }
 if (newPwd != confirm)
 {
 MessageBox.Show("Bevestiging komt niet overeen.", "Validatie", MessageBoxButton.OK, MessageBoxImage.Warning);
 return;
 }
 var result = await _userManager.ChangePasswordAsync(_user, oldPwd, newPwd);
 if (!result.Succeeded)
 {
 MessageBox.Show(string.Join("\n", result.Errors.Select(e => e.Description)), "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
 return;
 }
 MessageBox.Show("Wachtwoord gewijzigd.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
 DialogResult = true;
 Close();
 }
 }
}
