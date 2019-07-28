using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VinSeek.Utilities;

namespace VinSeek
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string fileName;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length != 0)
                fileName = e.Args[0];

            base.OnStartup(e);
        }
    }
}
