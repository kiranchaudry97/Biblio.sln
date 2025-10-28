// Doel: Code-behind voor inlogvenster; valideert invoer en voert authenticatie uit via UserManager.
// Beschrijving: Leest e-mail en wachtwoord, ondersteunt wachtwoord tonen en start resetflow. Stelt IsAuthenticated en LoggedInUser in.
using System;
using System.Windows;
using System.Threading.Tasks;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Net.Http.Json;
using Biblio.Dekstop.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Biblio.Dekstop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly HttpClient _http;
        private readonly ISecurityTokenProvider _tokenProvider;

        public bool IsAuthenticated { get; private set; }
        public AppUser? LoggedInUser { get; private set; }

        public LoginWindow(UserManager<AppUser> userManager, HttpClient http, ISecurityTokenProvider tokenProvider)
        {
            InitializeComponent();
            _userManager = userManager;
            _http = http;
            _tokenProvider = tokenProvider;
        }

        private void OnToggleShowPassword(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordCheck.IsChecked == true)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnForgotPassword(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var email = EmailBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vul eerst je e-mail in.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                MessageBox.Show("Geen account met dit e-mailadres gevonden.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // In desktop scenario: direct nieuw wachtwoord laten instellen zonder e-mailstap
            var wnd = new ResetPasswordWindow(_userManager, user)
            {
                Owner = this
            };
            wnd.ShowDialog();
        }

        private async void OnLogin(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            var email = EmailBox.Text?.Trim();
            var pwd = (ShowPasswordCheck.IsChecked == true) ? PasswordTextBox.Text : PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd))
            {
                ErrorText.Text = "Vul e-mail en wachtwoord in.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            //1) Try API token endpoint
            try
            {
                var resp = await _http.PostAsJsonAsync("api/auth/token", new { Email = email, Password = pwd });
                if (resp.IsSuccessStatusCode)
                {
                    var obj = await resp.Content.ReadFromJsonAsync<TokenResponse>();
                    if (obj != null && !string.IsNullOrWhiteSpace(obj.Token))
                    {
                        // parse token expiry
                        try
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jwt = handler.ReadJwtToken(obj.Token);
                            var exp = jwt.ValidTo;
                            if (exp < DateTime.UtcNow)
                            {
                                MessageBox.Show("Ontvangen token is al verlopen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch
                        {
                            // ignore parse errors
                        }

                        // Persist securely
                        _tokenProvider.SetToken(obj.Token);

                        // retrieve user info locally and set LoggedInUser
                        var user = await _userManager.FindByEmailAsync(email);
                        if (user != null)
                        {
                            LoggedInUser = user;
                            IsAuthenticated = true;
                            DialogResult = true;
                            Close();
                            return;
                        }
                    }
                }
            }
            catch
            {
                // ignore API errors and fallback to local Identity
            }

            //2) Fallback local check
            var localUser = await _userManager.FindByEmailAsync(email);
            if (localUser != null && await _userManager.CheckPasswordAsync(localUser, pwd))
            {
                LoggedInUser = localUser;
                IsAuthenticated = true;
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Aanmelden mislukt. Controleer je gegevens.";
                ErrorText.Visibility = Visibility.Visible;
            }
        }

        private record TokenResponse(string Token);
    }
}