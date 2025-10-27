using System;
using System.Windows;
using System.Windows.Controls;

namespace Biblio.Dekstop.Controls
{
    public partial class LabeledTextBox : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(LabeledTextBox), new PropertyMetadata(string.Empty));


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(LabeledTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }


        public LabeledTextBox() => InitializeComponent();
    }
}