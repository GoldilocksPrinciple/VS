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
using System.Collections.ObjectModel;
using Be.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Reflection;
using System.Windows.Interop;
using BeeSchema;
using System.Collections.Generic;
using System.Linq;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : System.Windows.Controls.UserControl
    {
        private MainWindow _mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        public ObservableCollection<VindictusPacket> PacketList;

        public VinSeekMainTab()
        {
            InitializeComponent();

            this.PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = PacketList;
        }

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

        #region Capture
        /// <summary>
        /// Start accepting packets from the packet provider
        /// </summary>
        public void StartCapturePackets()
        {
            this.PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = PacketList;

            Process[] ekinarProcess = Process.GetProcessesByName("Ekinar");

            if (ekinarProcess.Length < 0)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    CaptureStatusInfo.Text = "Ekinar is not running...";
                    _mainWindow.StartCaptureMenuItem.IsEnabled = false;
                }));
            }
            else
            {
                _mainWindow.PacketSource.AddHook(WndProc);

                Dispatcher.Invoke((Action)(() =>
                {
                    CaptureStatusInfo.Text = "Capturing data from Ekinar";
                    _mainWindow.StartCaptureMenuItem.IsEnabled = false;
                }));
            }
        }

        /// <summary>
        /// Stop accepting packets from the packet provider
        /// </summary>
        public void StopCapturePackets()
        {
            _mainWindow.PacketSource.RemoveHook(WndProc);

            Dispatcher.Invoke((Action)(() =>
            {
                CaptureStatusInfo.Text = "Stop capturing data";
                _mainWindow.StartCaptureMenuItem.IsEnabled = true;
            }));

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
        /// Parsing method for a template file on a selected packet buffer
        /// </summary>
        /// <param name="schemaPath">path to schema template file</param>
        /// <param name="packet">packet that needed to parse the template on</param>
        public void SchemaParser(string schemaPath, VindictusPacket packet)
        {
            try
            {
                var schema = Schema.FromFile(schemaPath);
                var result = schema.Parse(packet.Buffer);
                var model = ResultModel.CreateModel(this, result);
                Dispatcher.Invoke((Action)(() =>
                {
                    ParseResultTree.Model = model;
                }));
            }
            catch (Exception ex)
            {
                new ErrorConsoleView(ex.Message, "AN ERROR OCCURRED WHILE PARSING TEMPLATE").ShowDialog();
            }
        }

        /// <summary>
        /// Run template button clicked event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTemplate_Click(object sender, RoutedEventArgs e)
        {
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
                            var pack = new VindictusPacket(data, timestamp, false);
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
                PacketList[index].Note = new EditNoteView(PacketList[index].Note,
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
            this.PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = PacketList;
            try
            {
                var capture = XMLImporter.LoadCapture(path);
                foreach (var packet in capture.Packets)
                {
                    var vindiPacket = new VindictusPacket(packet.BufferWithDirection, packet.Time, true);
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
                    try
                    {
                        XMLImporter.SaveCapture(capture, saveDiag.FileName);
                        System.Windows.MessageBox.Show($"Capture successfully saved to {saveDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        saveDiag.Dispose();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }));
        }

        #endregion

        #region Ekinar interops
        /// <summary>
        /// Processing data received from packet provider
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x004A)
            {
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                byte[] buffer = new Byte[Marshal.ReadInt32(lParam, IntPtr.Size)];
                IntPtr dataPtr = Marshal.ReadIntPtr(lParam, IntPtr.Size * 2);
                Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                var packet = new VindictusPacket(buffer, timestamp, true);
                PacketList.Add(packet);
            }
            handled = false;
            return IntPtr.Zero;
        }
        #endregion
    }
}

