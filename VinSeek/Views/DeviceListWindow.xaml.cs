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
using System.Windows.Shapes;
using SharpPcap;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for DeviceListWindow.xaml
    /// </summary>
    public partial class DeviceListWindow : Window
    {
        public DeviceListWindow()
        {
            InitializeComponent();
        }

        private void DeviceListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var dev in CaptureDeviceList.Instance)
            {
                var str = String.Format("{0} {1}", dev.Name, dev.Description);
                DeviceList.Items.Add(str);
            }
        }

        public delegate void OnItemSelectedDelegate(int itemIndex);
        public event OnItemSelectedDelegate OnItemSelected;

        public delegate void OnCancelDelegate();
        public event OnCancelDelegate OnCancel;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                OnItemSelected(DeviceList.SelectedIndex);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel();
        }

        private void DeviceList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                OnItemSelected(DeviceList.SelectedIndex);
            }
        }
    }
}
