using System.Windows;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for EditNoteView.xaml
    /// </summary>
    public partial class EditNoteView : Window
    {
        public EditNoteView()
        {
            InitializeComponent();
        }
        public EditNoteView(string text, string info)
        {
            InitializeComponent();

            InfoLabel.Content = info;
            TextEditor.AppendText(text);
        }

        public new string ShowDialog()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.ShowDialog();

            return TextEditor.Text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
