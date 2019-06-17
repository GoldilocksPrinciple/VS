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
using WpfHexaEditor;
using System.IO;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : System.Windows.Controls.UserControl
    {
        private DeviceListWindow _deviceListWindow;
        private ICaptureDevice _device;
        private List<CapturedPackets> _capturedPacketsList;
        private MainWindow _mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
        private bool _backgroundThreadStop = false;

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
            /*new Thread(delegate ()
            {
                StartCapture(itemIndex);
            }).Start();*/
            await Task.Run(() => StartCapture(itemIndex));
        }

        private void StartCapture(int itemIndex) //TODO: Figure out why new capture didn't start after stopping the previous capture
        {
            Dispatcher.Invoke((Action)(() =>
            {
                _mainWindow.StartCaptureMenuItem.IsEnabled = false;
                PacketListView.ItemsSource = null;
            }));

            _capturedPacketsList = new List<CapturedPackets>();
            _device = CaptureDeviceList.Instance[itemIndex];

            // Open the device for capturing
            int readTimeoutMilliseconds = 0;
            _device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            _device.Filter = "tcp port 27015"; // Vindictus port

            RawCapture rawCapture;

            // Capture packets using GetNextPacket()
            while ((rawCapture = _device.GetNextPacket()) != null)
            {
                if (_backgroundThreadStop)
                {
                    _device.Close();
                    break;
                }
                Dispatcher.Invoke((Action)(() =>
                {
                    PacketListView.Items.Refresh();
                }));
                var pack = PacketDotNet.Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var tcp = (TcpPacket)pack.Extract(typeof(TcpPacket));
                if (tcp != null)
                {
                    IPPacket iPPacket = (IPPacket)tcp.ParentPacket;

                    // read packet information
                    var sourceIP = iPPacket.SourceAddress.ToString();
                    var destIP = iPPacket.DestinationAddress.ToString();
                    var protocol = iPPacket.Protocol.ToString();
                    var len = rawCapture.Data.Length.ToString();
                    var data = pack.PayloadPacket.PayloadPacket.PayloadData;

                    // update PacketListView
                    Dispatcher.Invoke((Action)(() =>
                    {
                        _capturedPacketsList.Add(new CapturedPackets() { SourceIP = sourceIP, DestIP = destIP, Protocol = protocol, Length = len, Data = data });
                        PacketListView.ItemsSource = _capturedPacketsList;
                        PacketListView.Items.Refresh();
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

        public class CapturedPackets
        {
            public string SourceIP { get; set; }
            public string DestIP { get; set; }
            public string Protocol { get; set; }
            public string Length { get; set; }
            public byte[] Data { get; set; }

        }
        
        private void PacketListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PacketListView.SelectedIndex == -1)
                return;

            if (_capturedPacketsList.Count == 0)
                return;

            var data = _capturedPacketsList[PacketListView.SelectedIndex].Data;

            MemoryStream selecteDataStream = new MemoryStream(data);

            LoadDataFromStream(selecteDataStream);
        }
    }
}

