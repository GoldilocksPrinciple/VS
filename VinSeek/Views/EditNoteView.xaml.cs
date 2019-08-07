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

        /// <summary>
        /// Create a window with corresponding information
        /// </summary>
        /// <param name="text">displayed note message</param>
        /// <param name="info">window's custom title</param>
        public EditNoteView(string text, string info)
        {
            InitializeComponent();

            InfoLabel.Content = info;
            TextEditor.AppendText(text);
        }

        /// <summary>
        /// Custom ShowDialog method for this window
        /// </summary>
        /// <returns></returns>
        public new string ShowDialog()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.ShowDialog();

            return TextEditor.Text;
        }

        /// <summary>
        /// Ok button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
