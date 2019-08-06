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
using System.Windows.Shapes;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for ErrorConsoleView.xaml
    /// </summary>
    public partial class ErrorConsoleView : Window
    {
        public ErrorConsoleView()
        {
            InitializeComponent();
            
        }

        public ErrorConsoleView(string message, string title)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            ErrorTextBox.Text = message;
            ErrorMessage.Content = title;
            System.Media.SystemSounds.Exclamation.Play();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
