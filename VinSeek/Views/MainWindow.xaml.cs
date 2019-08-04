using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using VinSeek.Model;
using VinSeek.Utilities;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TabItem item;
        public HwndSource PacketSource;

        public MainWindow()
        {
            InitializeComponent();

            // register custom file extension
            FileAssociationManager.SetAssociation(".vspcap", "VinSeek", FileAssociationManager.AssemmblyExecutablePath(), "VinSeek Packet Capture File");

            item = new TabItem();
            // create default new tab
            Dispatcher.Invoke((Action)(() =>
            {
                item.Content = new VinSeekMainTab();
                item.Header = "Start";
                MainTabControl.Items.Add(item);
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnSourceInitialized(e);
            this.PacketSource = PresentationSource.FromVisual(this) as HwndSource;
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
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
            {
                dialog.Title = "Select a XML capture file to open";
                dialog.Filters.Add(new CommonFileDialogFilter("XML File", "*.xml"));

                Dispatcher.Invoke((Action)(() =>
                {
                    if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                    {
                        var item = new TabItem();
                        item.Content = new VinSeekMainTab();
                        item.Header = System.IO.Path.GetFileName(dialog.FileName);
                        MainTabControl.Items.Add(item);
                        item.Focus();
                        ((VinSeekMainTab)item.Content).LoadCapture(dialog.FileName);
                    }
                }));
            }
        }

        private void SaveFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File.");
            ((VinSeekMainTab)item.Content).SaveCapture();
        }
        private void SaveFileAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File As.");
            ((VinSeekMainTab)item.Content).SaveCapture();
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Environment.Exit(0);
        }
        #endregion
    }
}
