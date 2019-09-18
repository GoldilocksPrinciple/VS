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
using System.Windows.Media.Imaging;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TabItem item;
        public int providerChoice = 0;
        public HwndSource EkinarPacketSource;

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

        /// <summary>
        /// Window loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnSourceInitialized(e);
            this.EkinarPacketSource = PresentationSource.FromVisual(this) as HwndSource;
        }
        
        #region Command Handlers
        /// <summary>
        /// New tab button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Open file button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Save file button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File.");
            ((VinSeekMainTab)item.Content).SaveCapture();
        }

        /// <summary>
        /// Save file as button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Save File As.");
            ((VinSeekMainTab)item.Content).SaveCapture();
        }

        /// <summary>
        /// Close tab button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.RemoveAt(MainTabControl.SelectedIndex);
        }

        /// <summary>
        /// Close all tab button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAllTabCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Close All Tab.");

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.Clear();
        }

        /// <summary>
        /// Exit application button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitApplicationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Exit Application.");
            Environment.Exit(0);
        }

        /// <summary>
        /// Start capture button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Stop capture button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopCaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Stop Capture.");

            if (MainTabControl.Items.Count == 0)
                return;

            if (StartCaptureMenuItem.IsEnabled)
                return;

            ((VinSeekMainTab)item.Content).StopCapturePackets();
        }

        /// <summary>
        /// Choose WinPCap as packet provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinPCapCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("WinPCap");

            if (!StartCaptureMenuItem.IsEnabled)
                return;

            WinPCapCheckMark.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/check_icon.ico"));
            EkinarCheckMark.Source = null;
            providerChoice = 0;
        }

        /// <summary>
        /// Choose Ekinar as packet provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EkinarCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Ekinar");

            if (!StartCaptureMenuItem.IsEnabled)
                return;

            WinPCapCheckMark.Source = null;
            EkinarCheckMark.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/check_icon.ico"));
            providerChoice = 1;
        }

        /// <summary>
        /// Open script button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open Script.");
        }

        /// <summary>
        /// Edit script button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Edit Script.");
        }

        /// <summary>
        /// Run script button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunScriptCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Run Script.");

            if (MainTabControl.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("No tab detected. Please create new tab before running script.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
        }

        /// <summary>
        /// Open template button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Open Template.");
        }

        /// <summary>
        /// Edit template button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Edit Template.");
        }

        /// <summary>
        /// Run template button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTemplateCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Run Template.");

            if (MainTabControl.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("No tab detected. Please create new tab before running template.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
        }
        
        /// <summary>
        /// Tab close button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabCloseClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Close Tab.");

            if (MainTabControl.Items.Count == 0)
                return;

            MainTabControl.Items.RemoveAt(MainTabControl.SelectedIndex);
        }

        /// <summary>
        /// Overide closing event of MainWindow
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Environment.Exit(0);
        }
        #endregion
    }
}
