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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using Be.Windows.Forms;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var item = new TabItem();
            item.Content = new VinSeekMainTab();
            item.Header = "New";
            MainTabControl.Items.Add(item);
        }

        #region Menu Items Click Handlers
        private void NewTab_Click(object sender, RoutedEventArgs e)
        {

        }
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CloseAll_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {

        }
        private void TabCloseClick(object sender, RoutedEventArgs e)
        {
            MainTabControl.Items.RemoveAt(MainTabControl.SelectedIndex);
        }
        #endregion

        public void OpenFile()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
            {
                dialog.Title = "Select a file to open";
                Debug.WriteLine(dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null);
                var item = new TabItem();
                item.Content = new VinSeekMainTab();
                item.Header = System.IO.Path.GetFileName(dialog.FileName);
                MainTabControl.Items.Add(item);
                ((VinSeekMainTab)item.Content).LoadData(dialog.FileName);
            }
        }
        
    }
}
