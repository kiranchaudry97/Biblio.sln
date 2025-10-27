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

namespace Biblio.Dekstop.Views
{
    public partial class LoansWindow : Window
    {
        public LoansWindow(LoansViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}

