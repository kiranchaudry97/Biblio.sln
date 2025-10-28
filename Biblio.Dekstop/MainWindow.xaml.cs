using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Biblio.Models.Entities;
using Biblio.Dekstop.Views;
using Biblio.Dekstop.Services;

namespace Biblio.Dekstop
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _sp;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISecurityTokenProvider _tokenProvider;
        private readonly SecurityViewModel _securityViewModel;

        public AppUser? CurrentUser { get; private set; }

        public MainWindow(IServiceProvider sp, UserManager<AppUser> userManager, ISecurityTokenProvider tokenProvider, SecurityViewModel securityViewModel)
        {
            InitializeComponent();
            _sp = sp;
            _userManager = userManager;
            _tokenProvider = tokenProvider;
            _securityViewModel = securityViewModel;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await PromptAndApplyRolesAsync();
                ApplyThemeMenuLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij initialisatie: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private async Task PromptAndApplyRolesAsync()
        {
            SecurityContext.Reset();
            _securityViewModel.Reset();
            var login = _sp.GetRequiredService<LoginWindow>();
            bool? result = login.ShowDialog();
            if (result == true && login.IsAuthenticated && login.LoggedInUser != null)
            {
                CurrentUser = login.LoggedInUser;
                SecurityContext.IsAdmin = await _userManager.IsInRoleAsync(CurrentUser, "Admin");
                SecurityContext.IsStaff = await _userManager.IsInRoleAsync(CurrentUser, "Medewerker");

                _securityViewModel.IsAdmin = SecurityContext.IsAdmin;
                _securityViewModel.IsStaff = SecurityContext.IsStaff;

                ApplyRoleVisibility();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ApplyRoleVisibility()
        {
            AdminMenu.Visibility = SecurityContext.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyThemeMenuLabel()
        {
            // Detecteer huidig thema en pas menu-item aan
            bool isDark = Application.Current.Resources.MergedDictionaries.Count > 0 &&
                          Application.Current.Resources.MergedDictionaries[0].Source?.ToString().EndsWith("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase) == true;
            if (DarkThemeMenuItem != null)
            {
                DarkThemeMenuItem.IsChecked = isDark;
                DarkThemeMenuItem.Header = isDark ? "Donker thema uitschakelen" : "Donker thema inschakelen";
            }
        }

        private async void OnLogout(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            SecurityContext.Reset();
            _securityViewModel.Reset();
            ApplyRoleVisibility();

            // clear token from provider
            try
            {
                _tokenProvider.SetToken(null);
            }
            catch { }

            MessageBox.Show("Je bent afgemeld.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            await PromptAndApplyRolesAsync();
        }

        private void OnExit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void OnManageUsers(object sender, RoutedEventArgs e)
        {
            var wnd = _sp.GetRequiredService<AdminUsersWindow>();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void OnOpenBooks(object sender, RoutedEventArgs e) => OpenWindowSafely<BooksWindow>("Boeken");
        private void OnOpenMembers(object sender, RoutedEventArgs e) => OpenWindowSafely<MembersWindow>("Leden");
        private void OnOpenLoans(object sender, RoutedEventArgs e) => OpenWindowSafely<LoansWindow>("Uitleningen");
        private void OnOpenCategories(object sender, RoutedEventArgs e) => OpenWindowSafely<CategoriesWindow>("Categorieën");

        private void OnOpenProfile(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null) { MessageBox.Show("Meld je eerst aan."); return; }
            var wnd = new ProfileWindow(_userManager, CurrentUser) { Owner = this };
            wnd.ShowDialog();
        }

        private void OnChangePassword(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null) { MessageBox.Show("Meld je eerst aan."); return; }
            var wnd = new ChangePasswordWindow(_userManager, CurrentUser) { Owner = this };
            wnd.ShowDialog();
        }

        private void OpenWindowSafely<T>(string titel) where T : Window
        {
            try
            {
                var wnd = _sp.GetRequiredService<T>();
                wnd.Owner = this;
                wnd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kon {titel}-venster niet openen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnToggleTheme(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            bool toDark = mi?.IsChecked == true;
            var rd = new ResourceDictionary
            {
                Source = new Uri(toDark ? "/Styles/Theme.Dark.xaml" : "/Styles/Theme.Light.xaml", UriKind.Relative)
            };
            if (Application.Current.Resources.MergedDictionaries.Count > 0)
                Application.Current.Resources.MergedDictionaries[0] = rd;
            else
                Application.Current.Resources.MergedDictionaries.Add(rd);

            // Update label naar in-/uitschakelen
            if (mi != null)
                mi.Header = toDark ? "Donker thema uitschakelen" : "Donker thema inschakelen";
        }
    }
}
