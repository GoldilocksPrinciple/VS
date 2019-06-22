using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using Machina;
using System.ComponentModel;
using VinSeek.Views;
using System.IO;
using System.Diagnostics;

namespace VinSeek.Model
{
    public class CapturedPacketInfo : INotifyPropertyChanged
    {
        private int _dataLength;
        private byte[] _data;
        
        public string Direction { get; set; }
        public string SourceIP { get; set; }
        public string DestIP { get; set; }
        public string SourcePort { get; set; }
        public string DestPort { get; set; }
        public IPProtocol Protocol { get; set; }
        public int DataLength
        {
            get { return _dataLength; }
            set
            {
                if (_dataLength != value)
                {
                    _dataLength = value;
                    OnPropertyChanged("DataLength");
                }
            }
        }

        public byte[] Data
        {
            get { return _data; }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged("Data");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
