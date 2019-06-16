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
using Be.Windows.Forms;

namespace VinSeek.Views
{
    /// <summary>
    /// Interaction logic for VinSeekMainTab.xaml
    /// </summary>
    public partial class VinSeekMainTab : UserControl
    {
        public VinSeekMainTab()
        {
            InitializeComponent();
        }

        public void LoadData(string fileName)
        {
            // Be.Windows.Forms.Hexbox
            /*DynamicFileByteProvider dynamicFileByteProvider = new DynamicFileByteProvider(fileName);
            HexEditor.ByteProvider = dynamicFileByteProvider;*/

            // WpfHexaEditor
            HexEditor.FileName = fileName;
        }
    }
}
