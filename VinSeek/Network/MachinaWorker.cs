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
        public PacketReassembler ReassemblerServer = new PacketReassembler();
        public PacketReassembler ReassemblerClient = new PacketReassembler();

        public MachinaWorker(VinSeekMainTab currentTab)
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

            var completedBuffer = ReassemblerServer.AnalyzePacket(data);

            if (completedBuffer != null)
            {
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                var packet = new VindictusPacket(completedBuffer, timestamp, "S");
                Debug.WriteLine("Packet: Opcode {0}. Length {1}", packet.Opcode, packet.PacketLength);
                _currentVinSeekTab.Dispatcher.Invoke(new Action(() => {
                    _currentVinSeekTab.PacketList.Add(packet);
                }));
            }
        }

        private void DataSent(string connection, TCPConnection tcpConnection, byte[] data)
        {
            if (tcpConnection.RemotePort.ToString() != "27015") // display filter
                return;

            var completedBuffer = ReassemblerClient.AnalyzePacket(data);

            if (completedBuffer != null)
            {
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                var packet = new VindictusPacket(completedBuffer, timestamp, "C");
                Debug.WriteLine("Packet: Opcode {0}. Length {1}", packet.Opcode, packet.PacketLength);
                _currentVinSeekTab.Dispatcher.Invoke(new Action(() => {
                    _currentVinSeekTab.PacketList.Add(packet);
                }));
            }
        }
    }
}