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

            PacketList = new ObservableCollection<VindictusPacket>();
            PacketListView.ItemsSource = PacketList;
        }

        public void LoadDataFromFile(string fileName)
        {
            HexBox.ByteProvider = new DynamicFileByteProvider(fileName);
        }

        public void LoadDataFromStream(byte[] data)
        {
            HexBox.ByteProvider = new DynamicByteProvider(data);
        }

        #region Machina
        public void StartCapturePackets()
        {
            PacketList = new ObservableCollection<VindictusPacket>();
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

        public void StopCapturePackets()
        {
            _mainWindow.PacketSource.RemoveHook(WndProc);

            Dispatcher.Invoke((Action)(() =>
            {
                CaptureStatusInfo.Text = "Stop capturing data";
                _mainWindow.StartCaptureMenuItem.IsEnabled = true;
            }));

        }

        public void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (PacketList.Count == 0)
                    return;

                UpdateSelectedItemHexBox(PacketListView.SelectedIndex);
            }));

        }

        public void UpdateSelectedItemHexBox(int index)
        {
            if (index == -1)
                return;

            var data = PacketList[index].Body;
            LoadDataFromStream(data);
        }
        #endregion

        #region Export, Import, Edit Packets
        private void ExportPacket_Click(object sender, RoutedEventArgs e)
        {
            /*
            var packets = PacketListView.SelectedItems;

            if (packets.Count == 0)
                return;

            if (packets.Count == 1)
            {
                if (packets.Count == 1)
                {
                    var packet = (CapturedPacketInfo)PacketListView.Items[PacketListView.SelectedIndex];

                    CommonSaveFileDialog exportDiag = new CommonSaveFileDialog();
                    exportDiag.AlwaysAppendDefaultExtension = true;
                    exportDiag.DefaultExtension = ".vspcap";
                    exportDiag.DefaultFileName = "MyCapture";
                    exportDiag.Filters.Add(new CommonFileDialogFilter("VinSeek Packet Capture File", "*.vspcap"));

                    Dispatcher.Invoke((Action)(() =>
                    {
                        if (exportDiag.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            File.WriteAllBytes(exportDiag.FileName, CustomPacketBuilder.BuildPacket(packet.SourceIP, packet.DestIP,
                                                                                                    packet.SourcePort, packet.DestPort, packet.Data));
                            System.Windows.MessageBox.Show($"Packet successfully saved to {exportDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
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
                                var currentDate = DateTime.Now.ToString("MM-dd-yy");
                                int count = 0;
                                foreach (CapturedPacketInfo packet in packets)
                                {
                                    File.WriteAllBytes(System.IO.Path.Combine(dialog.FileName, currentDate + "-CaptureNo" + count.ToString() + ".vspcap"),
                                                        CustomPacketBuilder.BuildPacket(packet.SourceIP, packet.DestIP,
                                                                                        packet.SourcePort, packet.DestPort, packet.Data));
                                    count++;
                                }
                                System.Windows.MessageBox.Show($"Packets successfully saved to {dialog.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            }
                        }));
                    }
                }
            }*/
        }

        private void ImportPacket_Click(object sender, RoutedEventArgs e)
        {
            /*
            Dispatcher.Invoke((Action)(() =>
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = false })
                {
                    dialog.Title = "Select a file to open";
                    // do nothing if no file is selected
                    if ((dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null) != null)
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            if (System.IO.Path.GetExtension(dialog.FileName) != ".vspcap") // if not file that can extract info -> only get the data dump
                            {
                                System.Windows.MessageBox.Show($"Error importing! {dialog.FileName} is not a VinSeek Packet Capture File. Please use open file function instead",
                                    "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            }
                            else
                            {
                                LoadPacketInfoFromFile(dialog.FileName);
                            }
                        }));
                    }
                }
            }));

            return;*/
        }

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

        
        public void LoadPacketInfoFromFile(string filename)
        {
            /*
            byte[] fileData = File.ReadAllBytes(filename);
            var pack = CustomPacketBuilder.ReadPacket(fileData);
            Dispatcher.Invoke(new ThreadStart(() => { PacketList.Add(pack); }));
            IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * pack.DataLength);
            Marshal.Copy(pack.Data, 0, intPtr, Marshal.SizeOf(typeof(byte)) * pack.DataLength);
            
            Marshal.FreeHGlobal(intPtr);*/
        }
        #endregion

        #region Ekinar interops
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x004A)
            {
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                byte[] buffer = new Byte[Marshal.ReadInt32(lParam, IntPtr.Size)];
                IntPtr dataPtr = Marshal.ReadIntPtr(lParam, IntPtr.Size * 2);
                Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                var packet = new VindictusPacket(buffer, timestamp);
                PacketList.Add(packet);
            }
            handled = false;
            return IntPtr.Zero;
        }
        #endregion
    }
}

