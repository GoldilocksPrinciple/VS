using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using VinSeek.Model;
using VinSeek.Utils;

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

            // register custom file extension
            FileAssociationManager.SetAssociation(".vspcap", "VinSeek", FileAssociationManager.AssemmblyExecutablePath(), "VinSeek Packet Capture File");

            // open by double click on packet file
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
                        else if (System.IO.Path.GetExtension(dialog.FileName) != ".pcap")
                        {
                            // TODO: Handle pcap file. Either use SharpPcap or PcapDotNet function to read pcap file
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

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.RemoveAt(MainTabControl.SelectedIndex);
        }
        private void CloseAllTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close All Tab.");

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.Clear();
        }
        private void ExitApplicationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Exit Application.");
            Environment.Exit(0);
        }
        private void StartCaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Start Capture.");

            if (MainTabControl.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("No tab detected. Please create new tab before starting capture.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            ((VinSeekMainTab)item.Content).StartCapturePackets();
        }
        private void StopCaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MainTabControl.Items.Count == 0)
                return;

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

            if (MainTabControl.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("No tab detected. Please create new tab before running script.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
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

            if (MainTabControl.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("No tab detected. Please create new tab before running template.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
        }
        private void TabCloseClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.RemoveAt(MainTabControl.SelectedIndex);
        }
        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Environment.Exit(0);
        }
    }
}
