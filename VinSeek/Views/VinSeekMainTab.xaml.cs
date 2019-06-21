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
using SharpPcap;
using PacketDotNet;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using WpfHexaEditor;
using System.IO;
using SharpPcap.LibPcap;
using VinSeek.Model;
using VinSeek.Utils;
using System.Collections.ObjectModel;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : System.Windows.Controls.UserControl
    {
        private MainWindow _mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        public ObservableCollection<CapturedPacketInfo> CapturedPacketsInfoList;
        private MemoryStream _selectedDataStream;
        private MachinaPacketCapture _captureWorker;
        private Thread _captureThread;
        private bool captureStarted;

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
        }

        public void LoadDataFromFile(string fileName)
        {
            HexEditor.FileName = fileName;
        }

        public void LoadDataFromStream(MemoryStream data)
        {
            HexEditor.Stream = data;
        }

        #region Machina
        public void StartCapturePackets()
        {
            if (_captureWorker != null)
                return;

            captureStarted = true;

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

        public void AddPacketToList(CapturedPacketInfo packetInfo)
        {
            CapturedPacketsInfoList.Add(packetInfo);

            Dispatcher.Invoke((Action)(() =>
            {
                PacketListView.Items.Add(packetInfo);

                // auto scroll to the end of the list when new item is added
                //PacketListView.ScrollIntoView(packetInfo);
            }));
        }

        public void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CapturedPacketsInfoList.Count == 0)
                return;

            UpdateSelectedItemHexBox(PacketListView.SelectedIndex);
        }

        public void UpdateSelectedItemHexBox(int index)
        {
            if (index == -1)
                return;

            var data = CapturedPacketsInfoList[index].Data;
            _selectedDataStream = new MemoryStream(data);
            LoadDataFromStream(_selectedDataStream);
        }

        public void UpdateNumberOfPackets(string direction, int count)
        {
            if (direction == "Init")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketSentText.Text = "Sent: " + count.ToString();
                    PacketReceivedText.Text = "Received: " + count.ToString();
                }));
            }
            else if (direction == "Sent")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketSentText.Text = "Sent: " + count.ToString();
                }));
            }
            else if (direction == "Received")
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketReceivedText.Text = "Received: " + count.ToString();
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

        #region Export Packets
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
                    exportDiag.DefaultExtension = ".dat";
                    exportDiag.DefaultFileName = "MyCapture";
                    exportDiag.Filters.Add(new CommonFileDialogFilter("Data files", "*.dat"));

                    if (exportDiag.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        File.WriteAllBytes(exportDiag.FileName, CustomPacketBuilder.BuildPacket(packet.Data));
                        System.Windows.MessageBox.Show($"Packet successfully saved to {exportDiag.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
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
                            System.Windows.MessageBox.Show($"Packets successfully saved to {dialog.FileName}.", "VinSeek", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }
                    }
                }
            }
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

