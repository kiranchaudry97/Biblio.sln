// Doel: Bootstrap van WPF-app met generieke host, DI, EF Core, Identity en seeding.
// Beschrijving: Start de Host, configureert services (DbContext, Identity, ViewModels/Windows),
// voert database seeding uit en toont het hoofdvenster. Bevat globale exception handlers.
using System;
using System.Net.Http.Headers;
using System.Windows;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using Biblio.Models.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Biblio.Dekstop.Views;
using Biblio.Dekstop.ViewModels;
using Biblio.Dekstop.Services;

namespace Biblio.Dekstop
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddUserSecrets<App>(optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Database
                    var connString = context.Configuration.GetConnectionString("DefaultConnection")
                                     ?? "Server=(localdb)\\MSSQLLocalDB;Database=BiblioDb;Trusted_Connection=True;MultipleActiveResultSets=true";
                    services.AddDbContext<BiblioDbContext>(options => options.UseSqlServer(connString));

                    // Identity
                    services.AddIdentityCore<AppUser>(options =>
                        {
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequireUppercase = false;
                            options.Password.RequiredLength = 6;
                        })
                        .AddRoles<IdentityRole>()
                        .AddEntityFrameworkStores<BiblioDbContext>();

                    // Seed options
                    services.AddOptions<SeedOptions>()
                        .Bind(context.Configuration.GetSection("Seed"))
                        .ValidateDataAnnotations();

                    // Windows
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<RegisterWindow>();
                    services.AddTransient<ChangePasswordWindow>();
                    services.AddTransient<ProfileWindow>();
                    services.AddTransient<AdminUsersWindow>();
                    services.AddTransient<BooksWindow>();
                    services.AddTransient<MembersWindow>();
                    services.AddTransient<LoansWindow>();
                    services.AddTransient<CategoriesWindow>();
                    services.AddTransient<AdminRestoreWindow>();

                    // ViewModels
                    services.AddTransient<BooksViewModel>();
                    services.AddTransient<MembersViewModel>();
                    services.AddTransient<LoansViewModel>();
                    services.AddTransient<CategoriesViewModel>();
                    services.AddSingleton<SecurityViewModel>();

                    // API client for Books (consumes Biblio.Api)
                    services.AddSingleton<ISecurityTokenProvider, SecureTokenProvider>();

                    services.AddHttpClient<IBookApiClient, BookApiClient>(client =>
                        {
                            var apiBase = context.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5001/";
                            client.BaseAddress = new Uri(apiBase);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        })
                        .AddHttpMessageHandler(sp =>
                        {
                            // handler that adds Authorization header when token is present
                            var tokenProvider = sp.GetRequiredService<ISecurityTokenProvider>();
                            return new AuthHeaderHandler(tokenProvider);
                        });

                    // also add registration for AuthHeaderHandler type
                    services.AddTransient<AuthHeaderHandler>();
                })
                .Build();

            // Global exception handling
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(args.Exception.Message, "Onverwachte fout", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    MessageBox.Show(ex.Message, "Onverwachte fout (achtergrond)", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // register SecurityViewModel instance into application resources for XAML binding
            var securityVm = AppHost.Services.GetRequiredService<SecurityViewModel>();
            Application.Current.Resources["SecurityViewModel"] = securityVm;

            try
            {
                await SeedData.InitializeAsync(AppHost.Services);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout tijdens database-initialisatie:\n{ex.Message}",
                    "Initialisatie", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var main = AppHost.Services.GetRequiredService<MainWindow>();
            main.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
