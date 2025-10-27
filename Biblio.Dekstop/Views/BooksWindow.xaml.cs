using System.Windows;
using Biblio.Dekstop.ViewModels;

namespace Biblio.Dekstop.Views
{
    public partial class BooksWindow : Window
    {
        public BooksWindow(BooksViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
