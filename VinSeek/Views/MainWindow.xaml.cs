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
using VinSeek.Model;
using VinSeek.Utils;
using Machina;
using System.Threading;
using System.Text.RegularExpressions;

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

            FileAssociationManager.SetAssociation(".vspcap", "VinSeek", FileAssociationManager.AssemmblyExecutablePath(), "VinSeek Packet Capture File");

            if (App.fileName != null)
            {
                var item = new TabItem();
                item.Content = new VinSeekMainTab();
                item.Header = System.IO.Path.GetFileName(App.fileName);
                MainTabControl.Items.Add(item);
                item.Focus();
                ((VinSeekMainTab)item.Content).LoadPacketInfoFromFile(App.fileName);
            }
            else
            {
                item = new TabItem();
                // create default new tab
                Dispatcher.Invoke((Action)(() =>
                {
                    item.Content = new VinSeekMainTab();
                    item.Header = "Start";
                    MainTabControl.Items.Add(item);
                }));
            }
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
                    item.Content = new VinSeekMainTab();
                    item.Header = System.IO.Path.GetFileName(dialog.FileName);
                    MainTabControl.Items.Add(item);
                    item.Focus();

                    ((VinSeekMainTab)item.Content).Dispatcher.Invoke((Action)(() =>
                    {
                        if (System.IO.Path.GetExtension(dialog.FileName) != ".vspcap") // if not file that can extract info -> only get the data dump
                        {
                            // load data into hex box
                            ((VinSeekMainTab)item.Content).LoadDataFromFile(dialog.FileName);
                        }
                        else
                        {
                            ((VinSeekMainTab)item.Content).LoadPacketInfoFromFile(dialog.FileName);
                        }
                    }));
                }
            }
        }
        
        #region Command Handlers
        private void NewTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("New Tab.");
            Dispatcher.Invoke((Action)(() =>
            {
                var item = new TabItem();
                item.Content = new VinSeekMainTab();
                item.Header = "New";
                MainTabControl.Items.Add(item);
                item.Focus();
            }));
        }
        private void OpenFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open File.");
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
        private void CloseTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");
            Dispatcher.Invoke((Action)(() =>
            {
                MainTabControl.Items.Remove(MainTabControl.SelectedIndex);
            }));
        }
        private void CloseAllTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close All Tab.");
        }
        private void ExitApplicationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Exit Application.");
            Environment.Exit(0);
        }
        private void StartCaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Start Capture.");
            ((VinSeekMainTab)item.Content).StartCapturePackets();
        }
        private void StopCaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (StartCaptureMenuItem.IsEnabled)
                return;

            Debug.WriteLine("Stop Capture.");
            ((VinSeekMainTab)item.Content).StopCapturePackets();

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
