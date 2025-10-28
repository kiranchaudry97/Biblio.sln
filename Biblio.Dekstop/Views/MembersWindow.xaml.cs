using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Biblio.Dekstop.ViewModels;
using Biblio.Dekstop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Biblio.Dekstop.Views
{
    public partial class MembersWindow : Window
    {
        public MembersWindow(MembersViewModel vm, SecurityViewModel security)
        {
            InitializeComponent();
            DataContext = vm;

            this.Resources["SecurityViewModel"] = security;
            DeleteButton.Visibility = security.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            security.PropertyChanged += (_, __) =>
            {
                DeleteButton.Visibility = security.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            };
        }
    }
}
