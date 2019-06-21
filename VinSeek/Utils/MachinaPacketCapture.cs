using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using VinSeek.Views;
using VinSeek.Model;
using System.Net;
using Machina;
using System.IO;

namespace VinSeek.Utils
{
    public class MachinaPacketCapture
    {
        private readonly VinSeekMainTab _currentVinSeekTab;
        private bool _stopCapturing;
        private int _packetSent = 0;
        private int _packetReceived = 0;
        private uint _processId;
        private int _streamId = 0;
        public bool foundProcessId = false;

        public MachinaPacketCapture(VinSeekMainTab currentTab)
        {
            this._currentVinSeekTab = currentTab;
        }

        public void Start()
        {
            _processId = 0;

            while (!_stopCapturing)
            {
                Process[] vindictusProcess = Process.GetProcessesByName("Vindictus");

                if (vindictusProcess.Length > 0) // can't be more than 1 instance right... 
                {
                    _processId = (uint)vindictusProcess[0].Id;
                    string info = "Listening for connection of Process [" + _processId.ToString() + "]";
                    _currentVinSeekTab.UpdateCaptureProcessInfo(info);
                    foundProcessId = true;
                    break;
                }
                else
                {
                    string info = "No Vindictus process found";
                    _currentVinSeekTab.UpdateCaptureProcessInfo(info);
                }
            }

            if (foundProcessId)
            {
                Debug.WriteLine("Starting capture monitor");
                TCPNetworkMonitor monitor = new TCPNetworkMonitor();
                monitor.ProcessID = _processId;
                monitor.MonitorType = TCPNetworkMonitor.NetworkMonitorType.WinPCap;
                monitor.DataReceived += (string connection, TCPConnection tcpConnection, byte[] data)
                                        => DataReceived(connection, tcpConnection, data);
                monitor.DataSent += (string connection, TCPConnection tcpConnection, byte[] data)
                                        => DataSent(connection, tcpConnection, data);
                monitor.Start();

                _currentVinSeekTab.UpdateNumberOfPackets("Init", 0);

                while (!_stopCapturing)
                {
                    Thread.Sleep(1);
                }

                monitor.Stop();
            }
        }

        public void Stop()
        {
            _stopCapturing = true;
            if (foundProcessId)
            {
                string info = "Listener stopped for Process [" + _processId.ToString() + "]";
                _currentVinSeekTab.UpdateCaptureProcessInfo(info);
            }
        }

        private void DataReceived(string connection, TCPConnection tcpConnection, byte[] data)
        {
            if (tcpConnection.RemotePort.ToString() != "27015") // display filter
                return;
            
            _packetReceived++;

            // first packet of first stream
            if (_currentVinSeekTab.CapturedPacketsInfoList.Count == 0)
            {
                var pack = NewCapturedPacketInfo(tcpConnection.LocalIP, tcpConnection.RemoteIP, tcpConnection.LocalPort, tcpConnection.RemotePort,
                                        data.Length, data, _streamId);

                _currentVinSeekTab.Dispatcher.Invoke(new Action(() => { _currentVinSeekTab.CapturedPacketsInfoList.Add(pack); }));
                //_currentVinSeekTab.AddPacketToList(pack);
            }
            else
            {
                bool oldStream = false;
                foreach(CapturedPacketInfo firstPacket in _currentVinSeekTab.CapturedPacketsInfoList)
                {
                    if (new IPAddress(tcpConnection.LocalIP).ToString() == firstPacket.LocalIP &&
                        new IPAddress(tcpConnection.RemoteIP).ToString() == firstPacket.RemoteIP &&
                        tcpConnection.LocalPort.ToString() == firstPacket.LocalPort &&
                        tcpConnection.RemotePort.ToString() == firstPacket.RemotePort) // check which stream this packet belongs to
                    {
                        oldStream = true;

                        // combine data 
                        byte[] buffer = new byte[data.Length + firstPacket.DataLength];
                        Array.Copy(firstPacket.Data, buffer, firstPacket.DataLength);
                        Array.Copy(data, 0, buffer, firstPacket.DataLength, data.Length);

                        // set new data and data length of current tcp stream
                        firstPacket.DataLength = data.Length + firstPacket.DataLength;
                        firstPacket.Data = buffer;
                    }
                }

                if (!oldStream) // if paket does not belong to old stream, add a new item to packet list view
                {
                    _streamId++;

                    var pack = NewCapturedPacketInfo(tcpConnection.LocalIP, tcpConnection.RemoteIP, tcpConnection.LocalPort, tcpConnection.RemotePort,
                                        data.Length, data, _streamId);

                    _currentVinSeekTab.Dispatcher.Invoke(new ThreadStart(() => { _currentVinSeekTab.CapturedPacketsInfoList.Add(pack); }));
                }
            }

            // update view for hexbox of current selected item in packet list view with new data
            _currentVinSeekTab.Dispatcher.Invoke(new ThreadStart(()
                                            => { _currentVinSeekTab.UpdateSelectedItemHexBox(_currentVinSeekTab.PacketListView.SelectedIndex); }));

            // update number of received packets
            _currentVinSeekTab.UpdateNumberOfPackets("Received", _packetReceived);
        }

        private void DataSent(string connection, TCPConnection tcpConnection, byte[] data)
        {
            if (tcpConnection.RemotePort.ToString() != "27015") // display filter
                return;

            _packetSent++;

            // Note: Does not need to check if sent packet belong to a new stream
            // New stream always starts with a packet from server, since all SYN and ACK packets have been filtered out
            foreach (CapturedPacketInfo firstPacket in _currentVinSeekTab.CapturedPacketsInfoList)
            {
                if (new IPAddress(tcpConnection.LocalIP).ToString() == firstPacket.LocalIP &&
                    new IPAddress(tcpConnection.RemoteIP).ToString() == firstPacket.RemoteIP &&
                    tcpConnection.LocalPort.ToString() == firstPacket.LocalPort &&
                    tcpConnection.RemotePort.ToString() == firstPacket.RemotePort) // check which stream this packet belongs to
                {

                    byte[] buffer = new byte[data.Length + firstPacket.DataLength];
                    Array.Copy(firstPacket.Data, buffer, firstPacket.DataLength);
                    Array.Copy(data, 0, buffer, firstPacket.DataLength, data.Length);

                    firstPacket.DataLength = data.Length + firstPacket.DataLength;
                    firstPacket.Data = buffer;
                }
            }

            // update view for hexbox of current selected item in packet list view with new data
            _currentVinSeekTab.Dispatcher.Invoke(new ThreadStart(()
                                            => { _currentVinSeekTab.UpdateSelectedItemHexBox(_currentVinSeekTab.PacketListView.SelectedIndex); }));

            // update number of sent packets
            _currentVinSeekTab.UpdateNumberOfPackets("Sent", _packetSent);
        }
    
        private CapturedPacketInfo NewCapturedPacketInfo(uint localIP, uint remoteIP, ushort localPort, ushort remotePort, 
                                                            int dataLength, byte[] data, int streamID)
        {
            // Direction is just for color display. Received = blue, Sent = orange
            var item = new CapturedPacketInfo
            {
                Direction = "Received",
                LocalIP = new IPAddress(localIP).ToString(),
                RemoteIP = new IPAddress(remoteIP).ToString(),
                LocalPort = localPort.ToString(),
                RemotePort = remotePort.ToString(),
                Protocol = IPProtocol.TCP,
                DataLength = dataLength,
                Data = data,
                StreamID = streamID
            };

            return item;
        }
    }
}
