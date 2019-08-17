using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinSeek.Views;
using VinSeek.Model;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Diagnostics;

namespace VinSeek.Utilities
{
    public enum FilterType
    {
        Opcode,
        PacketName,
        Port,
        Comment
    }

    // TODO: Handle bridge filter condition

    public class PacketFilter
    {
        private readonly VinSeekMainTab _currentVinSeekTab;
        private CollectionViewSource _filterSource;

        /// <summary>
        /// Constructor to pass current tab instance
        /// </summary>
        /// <param name="currentTab">current instance of vinseek tab</param>
        /// <param name="packetList">current packet list of the tab</param>
        public PacketFilter(VinSeekMainTab currentTab, ObservableCollection<VindictusPacket> packetList)
        {
            _currentVinSeekTab = currentTab;
            _filterSource = new CollectionViewSource();
            _filterSource.Source = packetList; // set view source to packet list
        }

        /// <summary>
        /// Set filter
        /// </summary>
        /// <param name="filterEntry">filter entry</param>
        public void SetFilter(string filterEntry)
        {
            FilterType filterType;
            string filterValue;

            if (filterEntry.Contains("packet.opcode"))
            {
                filterType = FilterType.Opcode;
                var splitString = filterEntry.Split(new[] { "==" }, StringSplitOptions.RemoveEmptyEntries); // split ==
                filterValue = splitString[1].Trim(); // delete whitespace
            }
            else if (filterEntry.Contains("packet.name"))
            {
                filterType = FilterType.PacketName;
                var splitString = filterEntry.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries); // split ==
                filterValue = splitString[1].Trim(); // delete whitespace
            }
            else if (filterEntry.Contains("tcp.port"))
            {
                filterType = FilterType.Port;
                var splitString = filterEntry.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries); // split ==
                filterValue = splitString[1].Trim(); // delete whitespace
            }
            else
            {
                filterType = FilterType.Comment;
                filterValue = filterEntry;
            }
            
            this.ApplyFilter(filterType, filterValue);
        }

        /// <summary>
        /// Apply filter
        /// </summary>
        /// <param name="filterType">filter type</param>
        /// <param name="filterValue">filter value</param>
        private void ApplyFilter(FilterType filterType, string filterValue)
        {
            switch(filterType)
            {
                case FilterType.Opcode:
                    // subscribe to opcode filter event handler
                    _filterSource.Filter += delegate (object sender, FilterEventArgs e) { this.Opcode_Filter(sender, e, filterValue); };

                    // update packet list view with applied filter
                    _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                    {
                        _currentVinSeekTab.PacketListView.ItemsSource = _filterSource.View;
                    }));

                    break;

                case FilterType.PacketName:
                    // subscribe to packet name filter event handler
                    _filterSource.Filter += delegate (object sender, FilterEventArgs e) { this.PacketName_Filter(sender, e, filterValue); };

                    // update packet list view with applied filter
                    _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                    {
                        _currentVinSeekTab.PacketListView.ItemsSource = _filterSource.View;
                    }));

                    break;
                case FilterType.Port:
                    // subscribe to port filter event handler
                    _filterSource.Filter += delegate (object sender, FilterEventArgs e) { this.Port_Filter(sender, e, filterValue); };

                    // update packet list view with applied filter
                    _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                    {
                        _currentVinSeekTab.PacketListView.ItemsSource = _filterSource.View;
                    }));

                    break;
                case FilterType.Comment:
                    //subscribe to comment event handler
                    _filterSource.Filter += delegate (object sender, FilterEventArgs e) { this.Comment_Filter(sender, e, filterValue); };

                    // update packet list view with applied filter
                    _currentVinSeekTab.Dispatcher.Invoke(new Action(() =>
                    {
                        _currentVinSeekTab.PacketListView.ItemsSource = _filterSource.View;
                    }));

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Filter event handler for opcode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="opcode"></param>
        private void Opcode_Filter(object sender, FilterEventArgs e, string opcode)
        {
            var packet = e.Item as VindictusPacket;
            if (packet.Opcode.ToString() == opcode)
                e.Accepted = true;
            else
                e.Accepted = false;
        }

        /// <summary>
        /// Filter event handler for packet name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="packetName"></param>
        private void PacketName_Filter(object sender, FilterEventArgs e, string packetName)
        {
            var packet = e.Item as VindictusPacket;
            if (packet.PacketName == packetName)
                e.Accepted = true;
            else
                e.Accepted = false;
        }

        /// <summary>
        /// Filter event handler for port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="port"></param>
        private void Port_Filter(object sender, FilterEventArgs e, string port)
        {
            var packet = e.Item as VindictusPacket;
            if (packet.ServerPort == port)
                e.Accepted = true;
            else
                e.Accepted = false;
        }

        /// <summary>
        /// Filter event handler for comment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="comment"></param>
        private void Comment_Filter(object sender, FilterEventArgs e, string comment)
        {
            var packet = e.Item as VindictusPacket;
            if (packet.Comment == comment)
                e.Accepted = true;
            else
                e.Accepted = false;
        }
    }
}
