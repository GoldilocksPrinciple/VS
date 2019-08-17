using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using VinSeek.Model;
using VinSeek.Utilities;
using VinSeek.Network;
using System.Collections.ObjectModel;
using Be.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Reflection;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : System.Windows.Controls.UserControl
    {
        private MainWindow _mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        public ObservableCollection<VindictusPacket> PacketList;
        private PacketFilter _packetFilter;
        private MachinaWorker _captureWorker;
        private Thread _captureThread;

        public VinSeekMainTab()
        {
            InitializeComponent();
            this.PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = this.PacketList;
            _packetFilter = new PacketFilter(this, this.PacketList);
        }

        #region Capture
        /// <summary>
        /// Start capturing network packets
        /// </summary>
        public void StartCapturePackets()
        {
            if (_captureWorker != null)
                return;

            this.PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = this.PacketList;
            _packetFilter = new PacketFilter(this, this.PacketList);

            _captureWorker = new MachinaWorker(this);
            _captureThread = new Thread(_captureWorker.Start);
            _captureThread.Start();

            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = false;
            }));
        }

        /// <summary>
        /// Stop capturing network packets
        /// </summary>
        public void StopCapturePackets()
        {
            if (_captureWorker == null)
                return;

            if (!_captureWorker.foundProcessId)
            {
                _captureWorker.Stop();

            }
            else
            {
                _captureWorker.Stop();
                _captureThread.Join();
            }
            
            _captureWorker = null;

            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = true;
            }));
        }
        #endregion

        #region Export, Import, Edit Packets
        /// <summary>
        /// Export packet button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportPacket_Click(object sender, RoutedEventArgs e)
        {
            var packets = PacketListView.SelectedItems;

            if (packets.Count == 0)
                return;

            if (packets.Count == 1)
            {
                var packet = (VindictusPacket)PacketListView.Items[PacketListView.SelectedIndex];

                CommonSaveFileDialog exportDiag = new CommonSaveFileDialog();
                exportDiag.AlwaysAppendDefaultExtension = true;
                exportDiag.DefaultExtension = ".bin";
                exportDiag.DefaultFileName = "MyPacket";
                exportDiag.Filters.Add(new CommonFileDialogFilter("Binary File", "*.bin"));

                Dispatcher.Invoke((Action)(() =>
                {
                    if (exportDiag.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        File.WriteAllBytes(exportDiag.FileName, packet.Buffer);
                        System.Windows.MessageBox.Show($"Packet successfully saved to {exportDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        exportDiag.Dispose();
                    }
                }));
            }
            else
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true })
                {
                    dialog.Title = "Select a folder to save packets";

                    Dispatcher.Invoke((Action)(() =>
                    {
                        if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                        {
                            foreach (VindictusPacket packet in packets)
                            {
                                File.WriteAllBytes(dialog.FileName, packet.Buffer);
                            }
                            System.Windows.MessageBox.Show($"Packets successfully saved", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            dialog.Dispose();
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// Import packet button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportPacket_Click(object sender, RoutedEventArgs e)
        {
            if (_captureWorker != null)
                return;

            Dispatcher.Invoke((Action)(() =>
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
                {
                    dialog.Title = "Select a file to open";
                    // do nothing if no file is selected
                    if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                    {
                        string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                        Dispatcher.Invoke((Action)(() =>
                        {
                            var data = File.ReadAllBytes(dialog.FileName);
                            var pack = new VindictusPacket(data, timestamp, "", "");
                            this.PacketList.Add(pack);
                        }));
                        dialog.Dispose();
                    }
                }
            }));
            return;
        }

        /// <summary>
        /// Edit note button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            var index = PacketListView.SelectedIndex;

            if (PacketList.Count == 0)
                return;

            if (index == -1)
                return;

            Dispatcher.Invoke((Action)(() =>
            {
                PacketList[index].Comment = new EditNoteView(PacketList[index].Comment,
                    "Enter text to edit comment/note for this packet. Click OK to save changes.").ShowDialog();
            }));
        }
        #endregion

        #region Load, Save Captures
        /// <summary>
        /// Load a selected capture file (XML) to view
        /// </summary>
        /// <param name="path">path to capture file to load</param>
        public void LoadCapture(string path)
        {
            try
            {
                var capture = XMLImporter.LoadCapture(path);
                foreach (var packet in capture.Packets)
                {
                    var vindiPacket = new VindictusPacket(packet.Buffer, packet.Time, packet.Direction, packet.ServerPort);
                    this.PacketList.Add(vindiPacket);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save current captured packets to a capture file (XML)
        /// </summary>
        public void SaveCapture()
        {
            if (PacketList.Count == 0)
                return;

            CommonSaveFileDialog saveDiag = new CommonSaveFileDialog();
            saveDiag.AlwaysAppendDefaultExtension = true;
            saveDiag.DefaultExtension = ".xml";
            saveDiag.DefaultFileName = "MyCapture";
            saveDiag.Filters.Add(new CommonFileDialogFilter("XML File", "*.xml"));

            Dispatcher.Invoke((Action)(() =>
            {
                if (saveDiag.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var capture = new Capture
                    {
                        Packets = PacketListView.Items.Cast<VindictusPacket>().ToArray()
                    };

                    XMLImporter.SaveCapture(capture, saveDiag.FileName);
                    System.Windows.MessageBox.Show($"Capture successfully saved to {saveDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    saveDiag.Dispose();
                }
            }));
        }

        #endregion

        #region Packet Filter
        /// <summary>
        /// Clear watermark text and change font color when first click on packet filter box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FilterBox.Text == "Apply a filter...")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    FilterBox.Text = "";
                    FilterBox.Foreground = new SolidColorBrush(Colors.Black);
                }));
            }
        }

        /// <summary>
        /// Click event handler for SET button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Set_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                FilterBox.Background = new SolidColorBrush(Colors.LightGreen);
            }));
            var filterText = FilterBox.Text;
            if (filterText != null)
            {
                _packetFilter.SetFilter(filterText);
            }
            else
                PacketListView.ItemsSource = this.PacketList;
        }

        /// <summary>
        /// Click event handler for CLEAR Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                FilterBox.Text = "Apply a filter...";
                FilterBox.Foreground = new SolidColorBrush(Colors.Gray);
                FilterBox.Background = new SolidColorBrush(Colors.White);
            }));
            PacketListView.ItemsSource = this.PacketList;
        }
        #endregion

        #region Hexbox
        /// <summary>
        /// Update hexbox using read all bytes from a file
        /// </summary>
        /// <param name="fileName">name of file</param>
        public void LoadDataFromFile(string fileName)
        {
            HexBox.ByteProvider = new DynamicFileByteProvider(fileName);
        }

        /// <summary>
        /// Update hexbox using byte arrays
        /// </summary>
        /// <param name="data">buffer</param>
        public void LoadDataFromStream(byte[] data)
        {
            HexBox.ByteProvider = new DynamicByteProvider(data);
        }
        #endregion

        /// <summary>
        /// Update capture status info
        /// </summary>
        /// <param name="text"></param>
        public void UpdateCaptureProcessInfo(string text, bool green)
        {
            if (green)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    CaptureStatusInfo.Text = text;
                    CaptureStatusInfo.Foreground = new SolidColorBrush(Colors.LimeGreen);
                }));
            }
            else
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    CaptureStatusInfo.Text = text;
                    CaptureStatusInfo.Foreground = new SolidColorBrush(Colors.Red);
                }));
            }
        }

        /// <summary>
        /// Update hexbox using correct buffer of selected packet in PacketListView
        /// </summary>
        /// <param name="index">index of selected item</param>
        public void UpdateSelectedItemHexBox(int index)
        {
            if (index == -1)
                return;

            var data = PacketList[index].Buffer;
            this.LoadDataFromStream(data);
        }

        /// <summary>
        /// PacketListView selected item changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (this.PacketList.Count == 0)
                    return;

                this.UpdateSelectedItemHexBox(PacketListView.SelectedIndex);
            }));
        }

        /// <summary>
        /// Run template button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTemplate_Click(object sender, RoutedEventArgs e)
        {
            /*
            var packets = PacketListView.SelectedItems;

            if (packets.Count == 0)
                return;

            if (packets.Count > 1)
                return;

            var packet = (VindictusPacket)PacketListView.Items[PacketListView.SelectedIndex];

            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
            {
                dialog.Title = "Select a file to open";
                // do nothing if no file is selected
                if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                {
                    this.SchemaParser(dialog.FileName, packet);
                    dialog.Dispose();
                }
            }*/
        }
    }
}

