using System.Windows;
using Biblio.Dekstop.ViewModels;
using Biblio.Dekstop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Biblio.Dekstop.Views
{
    public partial class BooksWindow : Window
    {
        public BooksWindow(BooksViewModel vm, SecurityViewModel security)
        {
            InitializeComponent();
            DataContext = vm;

            // Bind DeleteButton visibility to security viewmodel
            this.Resources["SecurityViewModel"] = security;
            DeleteButton.Visibility = security.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            security.PropertyChanged += (_, __) =>
            {
                DeleteButton.Visibility = security.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            };
        }
    }
}
