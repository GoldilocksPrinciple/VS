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

namespace VinSeek.Utils
{
    public class MachinaPacketCapture
    {
        private readonly VinSeekMainTab _currentVinSeekTab;
        private bool _stopCapturing;
        private int _packetSent = 0;
        private int _packetReceived = 0;
        private uint _processId;
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

                if (vindictusProcess.Length > 0) // can't be more than 1 instance... 
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
                monitor.DataReceived += (string connection, TCPConnection tcpConnection, byte[] data) => DataReceived(connection, tcpConnection, data);
                monitor.DataSent += (string connection, TCPConnection tcpConnection, byte[] data) => DataSent(connection, tcpConnection, data);
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
            if (tcpConnection.RemotePort.ToString() == "27015")
            {
                var item = new CapturedPacketInfo
                {
                    Direction = "Received",
                    SourceIP = new IPAddress(tcpConnection.RemoteIP).ToString(),
                    DestIP = new IPAddress(tcpConnection.LocalIP).ToString(),
                    SourcePort = tcpConnection.RemotePort.ToString(),
                    DestPort = tcpConnection.LocalPort.ToString(),
                    Protocol = IPProtocol.TCP,
                    Length = data.Length,
                    Data = data,
                };
                _packetReceived++;
                _currentVinSeekTab.AddPacketToList(item);
                _currentVinSeekTab.UpdateNumberOfPackets("Received", _packetReceived);
            }
        }

        private void DataSent(string connection, TCPConnection tcpConnection, byte[] data)
        {
            if (tcpConnection.RemotePort.ToString() == "27015")
            {
                var item = new CapturedPacketInfo
                {
                    Direction = "Sent",
                    SourceIP = new IPAddress(tcpConnection.LocalIP).ToString(),
                    DestIP = new IPAddress(tcpConnection.RemoteIP).ToString(),
                    SourcePort = tcpConnection.LocalPort.ToString(),
                    DestPort = tcpConnection.RemotePort.ToString(),
                    Protocol = IPProtocol.TCP,
                    Length = data.Length,
                    Data = data,
                };
                _packetSent++;
                _currentVinSeekTab.AddPacketToList(item);
                _currentVinSeekTab.UpdateNumberOfPackets("Sent", _packetSent);
            }
        }
    }
}
