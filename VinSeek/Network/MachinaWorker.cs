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

namespace VinSeek.Network
{
    public class MachinaWorker
    {
        private readonly VinSeekMainTab _currentVinSeekTab;
        private bool _stopCapturing;
        private uint _processId;
        public bool foundProcessId = false;
        private static PacketHandler _packetHandlerServerWorld;
        private static PacketHandler _packetHandlerClientWorld;

        public MachinaWorker(VinSeekMainTab currentTab)
        {
            _currentVinSeekTab = currentTab;
            _packetHandlerServerWorld = new PacketHandler(_currentVinSeekTab, "S");
            _packetHandlerClientWorld = new PacketHandler(_currentVinSeekTab, "C");
        }

        public void Start()
        {
            _processId = 0;

            while (!_stopCapturing)
            {
                Process[] vindictusProcess = Process.GetProcessesByName("Vindictus");
                Process[] heroesProcess = Process.GetProcessesByName("heroes");

                if (vindictusProcess.Length > 0 || heroesProcess.Length > 0) // can't be more than 1 instance right... 
                {
                    if (vindictusProcess.Length > 0)
                        _processId = (uint)vindictusProcess[0].Id;
                    else
                        _processId = (uint)heroesProcess[0].Id;

                    string info = "Listening for connection of Process [" + _processId.ToString() + "]";
                    _currentVinSeekTab.UpdateCaptureProcessInfo(info, true);
                    foundProcessId = true;
                    break;
                }
                else
                {
                    string info = "No Vindictus process found";
                    _currentVinSeekTab.UpdateCaptureProcessInfo(info, false);
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
                _currentVinSeekTab.UpdateCaptureProcessInfo(info, false);
            }
        }

        private void DataReceived(string connection, TCPConnection tcpConnection, byte[] data)
        {
            // 27015 = world, 27005 = dungeon
            if (tcpConnection.RemotePort.ToString() == "27015")
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                _packetHandlerServerWorld.AnalyzePacket(buffer, true);
            }

            /*if (tcpConnection.RemotePort.ToString() == "27005") // TODO: Figure out packet format of these
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                var packet = new VindictusPacket(buffer, timestamp, "S", "27005", 0);
                _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                {
                    _currentVinSeekTab.PacketList.Add(packet);
                }));
            }*/

            /*if (tcpConnection.RemotePort.ToString() == "27023")
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                _packetHandlerServerWorld.AnalyzePacket(buffer, false);
            }*/
        }

        private void DataSent(string connection, TCPConnection tcpConnection, byte[] data)
        {
            // 27015 = world, 27005 = dungeon
            if (tcpConnection.RemotePort.ToString() == "27015")
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                _packetHandlerClientWorld.AnalyzePacket(buffer, true);
            }

            /*if (tcpConnection.RemotePort.ToString() == "27005") // TODO: Figure out packet format of these
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                var packet = new VindictusPacket(buffer, timestamp, "C", "27005", 0);
                _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                {
                    _currentVinSeekTab.PacketList.Add(packet);
                }));
            }*/

            /*if (tcpConnection.RemotePort.ToString() == "27023")
            {
                var buffer = new byte[data.Length];
                Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                _packetHandlerClientWorld.AnalyzePacket(buffer, false);
            }*/
        }
    }
}