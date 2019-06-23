using System;
using System.Windows;
using System.Windows.Controls;
using SharpPcap;
using PacketDotNet;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using SharpPcap.LibPcap;
using VinSeek.Model;
using VinSeek.Utils;
using VinSeek.Network;
using System.Collections.ObjectModel;
using Machina;
using Be.Windows.Forms;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : System.Windows.Controls.UserControl
    {
        private MainWindow _mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        public ObservableCollection<CapturedPacketInfo> CapturedPacketsInfoList;
        private MachinaPacketCapture _captureWorker;
        private Thread _captureThread;

        // SharpPcap variables
        /*private List<CapturedPacketsInfo> _capturedPacketsInfoList;
        public bool captureStarted = false;
        private DeviceListWindow _deviceListWindow;
        private ICaptureDevice _device;
        private bool _backgroundThreadStop = false;*/

        //public ObservableCollection<CapturedPacketInfo> CapturedPacketsList { get; set; }

        public VinSeekMainTab()
        {
            InitializeComponent();

            CapturedPacketsInfoList = new ObservableCollection<CapturedPacketInfo>();
            PacketListView.ItemsSource = CapturedPacketsInfoList;
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
            if (_captureWorker != null)
                return;

            CapturedPacketsInfoList = new ObservableCollection<CapturedPacketInfo>();
            PacketListView.ItemsSource = CapturedPacketsInfoList;

            _captureWorker = new MachinaPacketCapture(this);
            _captureThread = new Thread(_captureWorker.Start);
            _captureThread.Start();

            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = false;
            }));
        }

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

        public void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (CapturedPacketsInfoList.Count == 0)
                    return;

                UpdateSelectedItemHexBox(PacketListView.SelectedIndex);
            }));

        }

        public void UpdateSelectedItemHexBox(int index)
        {
            if (index == -1)
                return;

            var data = CapturedPacketsInfoList[index].Data;
            LoadDataFromStream(data);
        }

        public void UpdateNumberOfPackets(string direction, int count)
        {
            if (direction == "Init")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketSentText.Text = "Client: " + count.ToString();
                    PacketReceivedText.Text = "Server: " + count.ToString();
                }));
            }
            else if (direction == "Sent")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketSentText.Text = "Client: " + count.ToString();
                }));
            }
            else if (direction == "Received")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketReceivedText.Text = "Server: " + count.ToString();
                }));
            }
            else
                return;
        }

        public void UpdateCaptureProcessInfo(string text)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ProcessInfoText.Text = text;
            }));
        }
        #endregion

        #region Export, Import, Edit Packets
        private void ExportPacket_Click(object sender, RoutedEventArgs e)
        {
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
            }
        }

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

            return;
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            var index = PacketListView.SelectedIndex;

            if (CapturedPacketsInfoList.Count == 0)
                return;

            if (index == -1)
                return;

            Dispatcher.Invoke((Action)(() =>
            {
                CapturedPacketsInfoList[index].Note = new EditNoteView(CapturedPacketsInfoList[index].Note,
                    "Enter text to edit comment/note for this packet. Click OK to save changes.").ShowDialog();
            }));
        }

        public void LoadPacketInfoFromFile(string filename)
        {
            byte[] fileData = File.ReadAllBytes(filename);
            var pack = CustomPacketBuilder.ReadPacket(fileData);
            Dispatcher.Invoke(new ThreadStart(() => { CapturedPacketsInfoList.Add(pack); }));
        }
        #endregion

        #region SharpPcap
        /*
        public void StartCapturePackets()
        {
            _deviceListWindow = new DeviceListWindow();
            _deviceListWindow.Show();
            _deviceListWindow.OnItemSelected += DeviceListWindow_OnItemSelected;
            _deviceListWindow.OnCancel += DeviceListWindow_OnCancel;
        }

        private void DeviceListWindow_OnCancel()
        {
            Environment.Exit(0);
        }

        private async void DeviceListWindow_OnItemSelected(int itemIndex)
        {
            _deviceListWindow.Hide();
            new Thread(delegate ()
            {
                StartCapture(itemIndex);
            }).Start();
            await Task.Run(() => StartCapture(itemIndex));
        }

        private void StartCapture(int itemIndex) //TODO: Figure out why new capture didn't start after stopping the previous capture
        {
            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = false;
                PacketListView.ItemsSource = null;
            }));

            _capturedPacketsInfoList = new List<CapturedPacketsInfo>();
            _device = CaptureDeviceList.Instance[itemIndex];

            // Open the device for capturing
            int readTimeoutMilliseconds = 0;
            _device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            _device.Filter = "tcp port 27015"; // Vindictus port

            RawCapture rawCapture;

            int packetSent = 0;
            int packetReceived = 0;

            // Capture packets using GetNextPacket()
            while ((rawCapture = _device.GetNextPacket()) != null)
            {
                if (_backgroundThreadStop)
                {
                    _device.Close();
                    break;
                }

                var pack = PacketDotNet.Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var tcp = (TcpPacket)pack.Extract(typeof(TcpPacket));
                if (tcp != null)
                {
                    IPPacket iPPacket = (IPPacket)tcp.ParentPacket;

                    // read packet information
                    string direction;
                    var sourceIP = iPPacket.SourceAddress.ToString();
                    var destIP = iPPacket.DestinationAddress.ToString();
                    var sourcePort = tcp.SourcePort.ToString();
                    var destPort = tcp.DestinationPort.ToString();
                    var protocol = iPPacket.Protocol.ToString();
                    var len = rawCapture.Data.Length.ToString();
                    var data = pack.PayloadPacket.PayloadPacket.PayloadData;

                    if (sourcePort == "27015")
                    {
                        direction = "Received";
                        packetReceived++;
                    }
                    else
                    {
                        direction = "Sent";
                        packetSent++;
                    }

                    UpdateNumberOfPackets("Sent", packetSent);
                    UpdateNumberOfPackets("Received", packetReceived);

                    Debug.WriteLine(rawCapture.Data.Length.GetType());

                    var packet = new CapturedPacketsInfo() { Direction = direction, SourceIP = sourceIP, DestIP = destIP, SourcePort = sourcePort, DestPort = destPort, Protocol = protocol, Length = len, Data = data };
                    _capturedPacketsInfoList.Add(packet);
                    // update PacketListView
                    Dispatcher.Invoke((Action)(() =>
                    {
                        PacketListView.Items.Add(packet);
                    }));
                }
            }
        }

        public void StopCapturePackets()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = true;
            }));
            _backgroundThreadStop = true;
        }
        
        public class CapturedPacketsInfo
        {
            public string Direction { get; set; }
            public string SourceIP { get; set; }
            public string DestIP { get; set; }
            public string SourcePort { get; set; }
            public string DestPort { get; set; }
            public string Protocol { get; set; }
            public string Length { get; set; }
            public byte[] Data { get; set; }

        }
        
        private void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PacketListView.SelectedIndex == -1)
                return;

            if (_capturedPacketsInfoList.Count == 0)
                return;

            var data = _capturedPacketsInfoList[PacketListView.SelectedIndex].Data;

            _selectedDataStream = new MemoryStream(data);

            Dispatcher.Invoke((Action)(() =>
            {
                LoadDataFromStream(_selectedDataStream);
            }));
        }*/
        #endregion
    }
}

