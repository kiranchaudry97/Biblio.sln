// Doel: Window code-behind dat de CategoriesViewModel injecteert en aan DataContext bindt.
using System.Windows;
using Biblio.Dekstop.ViewModels;

namespace Biblio.Dekstop.Views
{
 public partial class CategoriesWindow : Window
 {
 public CategoriesWindow(CategoriesViewModel vm)
 {
 InitializeComponent();
 DataContext = vm;
 }
 }
}
