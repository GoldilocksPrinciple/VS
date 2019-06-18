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
        //private int packetSent = 0;
        //private int packetReceived = 0;

        public MachinaPacketCapture(VinSeekMainTab currentTab)
        {
            this._currentVinSeekTab = currentTab;
        }

        public void Start()
        {
            uint processId;

            while (true)
            {
                Process[] vindictusProcess = Process.GetProcessesByName("Vindictus");

                if (vindictusProcess.Length > 0)
                {
                    processId = (uint)vindictusProcess[0].Id;
                    Debug.WriteLine("Found a Vindictus process. ID: " + processId.ToString());
                    break;
                }
            }

            Debug.WriteLine("Starting capture monitor");
            TCPNetworkMonitor monitor = new TCPNetworkMonitor();
            monitor.ProcessID = processId;
            monitor.MonitorType = TCPNetworkMonitor.NetworkMonitorType.WinPCap;
            monitor.DataReceived += (string connection, TCPConnection tcpConnection, byte[] data, int packetLength) => DataReceived(connection, tcpConnection, data, packetLength);
            monitor.DataSent += (string connection, TCPConnection tcpConnection, byte[] data, int packetLength) => DataSent(connection, tcpConnection, data, packetLength);
            monitor.Start();

            while (!_stopCapturing)
            {
                Thread.Sleep(1);
            }

            monitor.Stop();
        }

        public void Stop()
        {
            _stopCapturing = true;
        }

        private void DataReceived(string connection, TCPConnection tcpConnection, byte[] data, int packetLength)
        {
            Debug.WriteLine("Received TCP Packets.");
            var item = new CapturedPacketInfo
            {
                Direction = "Received",
                SourceIP = new IPAddress(tcpConnection.RemoteIP).ToString(),
                DestIP = new IPAddress(tcpConnection.LocalIP).ToString(),
                SourcePort = tcpConnection.RemotePort.ToString(),
                DestPort = tcpConnection.LocalPort.ToString(),
                Protocol = IPProtocol.TCP,
                Length = packetLength,
                Data = data,
            };
            _currentVinSeekTab.AddPacketToList(item);
            //packetReceived++;
        }

        private void DataSent(string connection, TCPConnection tcpConnection, byte[] data, int packetLength)
        {
            Debug.WriteLine("Sending TCP Packets.");
            var item = new CapturedPacketInfo
            {
                Direction = "Sent",
                SourceIP = new IPAddress(tcpConnection.LocalIP).ToString(),
                DestIP = new IPAddress(tcpConnection.RemoteIP).ToString(),
                SourcePort = tcpConnection.LocalPort.ToString(),
                DestPort = tcpConnection.RemotePort.ToString(),
                Protocol = IPProtocol.TCP,
                Length = packetLength,
                Data = data,
            };
            _currentVinSeekTab.AddPacketToList(item);
            //packetSent++;
        }

    }
}
