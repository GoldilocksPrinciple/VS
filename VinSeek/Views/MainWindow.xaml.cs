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

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TabItem item;
        public MainWindow()
        {
            InitializeComponent();
            item = new TabItem();
            // create new tab for the opened file
            item.Content = new VinSeekMainTab();
            item.Header = "New";
            MainTabControl.Items.Add(item);
        }

        public void OpenFile()
        {
            // file picker dialog
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
            {
                dialog.Title = "Select a file to open";
                // do nothing if no file is selected
                if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                {
                    var item = new TabItem();
                    // create new tab for the opened file
                    item.Content = new VinSeekMainTab();
                    item.Header = System.IO.Path.GetFileName(dialog.FileName);
                    MainTabControl.Items.Add(item);
                    item.Focus();
                    // load data into hex box
                    ((VinSeekMainTab)item.Content).LoadDataFromFile(dialog.FileName);
                }
            }
        }

        #region Command Handlers
        private void NewTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("New Tab.");
        }
        private void OpenFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File.");
        }
        private void SaveFileAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File As.");
        }
        private void ExportPacketsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Export Packets.");
        }
        private void CloseTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");
        }
        private void CloseAllTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close All Tab.");
        }
        private void ExitApplicationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Exit Application.");
        }
        private void CaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (StartCaptureMenuItem.IsEnabled == true)
            {
                ((VinSeekMainTab)item.Content).StartCapturePackets();
                Debug.WriteLine("Start Capture.");
            }
            else
            {
                ((VinSeekMainTab)item.Content).StopCapturePackets();
                Debug.WriteLine("Stop Capture.");
            }
                
        }
        private void OpenScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open Script.");
        }
        private void EditScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Edit Script.");
        }
        private void RunScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Run Script.");
        }
        private void OpenTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open Template.");
        }
        private void EditTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Edit Template.");
        }
        private void RunTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Run Template.");
        }
        private void TabCloseClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");
        }
        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Environment.Exit(0);
        }
    }
}
