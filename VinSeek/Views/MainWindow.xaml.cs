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
            // create default new tab
            Dispatcher.Invoke((Action)(() =>
            {
                item.Content = new VinSeekMainTab();
                item.Header = "New";
                MainTabControl.Items.Add(item);
            }));
            
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
                    Dispatcher.Invoke((Action)(() =>
                    {
                        item.Content = new VinSeekMainTab();
                        item.Header = System.IO.Path.GetFileName(dialog.FileName);
                        MainTabControl.Items.Add(item);
                        item.Focus();
                        // load data into hex box
                        ((VinSeekMainTab)item.Content).LoadDataFromFile(dialog.FileName);
                    }));
                }
            }
        }

        public void ExportPacketsToFile()
        {
            if (!StartCaptureMenuItem.IsEnabled)
                return;

            var packets = ((VinSeekMainTab)item.Content).CapturedPacketsList;

            if (packets.Count == 0)
                return;

            Debug.WriteLine("Packet count = " + packets.Count.ToString());

            if (packets.Count == 1)
            {
                var packet = ((VinSeekMainTab)item.Content).CapturedPacketsList[0];

                CommonSaveFileDialog exportDiag = new CommonSaveFileDialog();
                exportDiag.AlwaysAppendDefaultExtension = true;
                exportDiag.DefaultExtension = ".dat";
                exportDiag.DefaultFileName = "MyCapture";
                exportDiag.Filters.Add(new CommonFileDialogFilter("Data files", "*.dat"));

                if (exportDiag.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    File.WriteAllBytes(exportDiag.FileName, CustomPacketBuilder.BuildPacket(packet.Data));
                    MessageBox.Show($"Packet successfully saved to {exportDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
            else
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true })
                {
                    dialog.Title = "Select a folder to save packets";

                    if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                    {
                        int count = 0;
                        var currentDate = DateTime.Now.ToString("MM-dd-yy");
                        foreach (CapturedPacketInfo packet in packets)
                        {
                            File.WriteAllBytes(System.IO.Path.Combine(dialog.FileName, currentDate + "-CaptureNo" + count.ToString() + ".dat"), CustomPacketBuilder.BuildPacket(packet.Data));
                            count++;
                        }
                        MessageBox.Show($"Packets successfully saved to {dialog.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
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
        private void ExportPacketsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Export Packets.");
            ExportPacketsToFile();
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
            Environment.Exit(0);
        }
        private void CaptureCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (StartCaptureMenuItem.IsEnabled)
            {
                Debug.WriteLine("Start Capture.");
                ((VinSeekMainTab)item.Content).StartCapturePackets();
            }
            else
            {
                Debug.WriteLine("Stop Capture.");
                ((VinSeekMainTab)item.Content).StopCapturePackets();
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
