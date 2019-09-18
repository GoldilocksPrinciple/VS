using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VinSeek.Utilities
{
    public static class ShortcutCommands
    {
        static RoutedUICommand newTab = new RoutedUICommand("New Tab", "NewTab", typeof(ShortcutCommands));
        static RoutedUICommand openFile = new RoutedUICommand("Open File...", "OpenFile", typeof(ShortcutCommands));
        static RoutedUICommand saveFile = new RoutedUICommand("Save", "SaveFile", typeof(ShortcutCommands));
        static RoutedUICommand saveFileAs = new RoutedUICommand("Save As...", "SaveFileAs", typeof(ShortcutCommands));
        static RoutedUICommand closeTab = new RoutedUICommand("Close", "CloseTab", typeof(ShortcutCommands));
        static RoutedUICommand closeAllTab = new RoutedUICommand("Close All", "CloseAllTab", typeof(ShortcutCommands));
        static RoutedUICommand startCapturePackets = new RoutedUICommand("Start", "StartCapture", typeof(ShortcutCommands));
        static RoutedUICommand stopCapturePackets = new RoutedUICommand("Stop", "StopCapture", typeof(ShortcutCommands));
        static RoutedUICommand winpcap = new RoutedUICommand("WinPCap", "WinPCap", typeof(ShortcutCommands));
        static RoutedUICommand ekinar = new RoutedUICommand("Ekinar", "Ekinar", typeof(ShortcutCommands));
        static RoutedUICommand openScript = new RoutedUICommand("Open Script...", "OpenScript", typeof(ShortcutCommands));
        static RoutedUICommand editScript = new RoutedUICommand("Edit Script", "EditScript", typeof(ShortcutCommands));
        static RoutedUICommand runScript = new RoutedUICommand("Run Script", "RunScript", typeof(ShortcutCommands));
        static RoutedUICommand openTemplate = new RoutedUICommand("Open Template...", "OpenTemplate", typeof(ShortcutCommands));
        static RoutedUICommand editTemplate = new RoutedUICommand("Edit Template", "EditTemplate", typeof(ShortcutCommands));
        static RoutedUICommand runTemplate = new RoutedUICommand("Run Template", "RunTemplate", typeof(ShortcutCommands));
        static RoutedUICommand exitApplication = new RoutedUICommand("Exit", "ExitApplication", typeof(ShortcutCommands));

        public static RoutedUICommand NewTab { get { return newTab; } }
        public static RoutedUICommand OpenFile { get { return openFile; } }
        public static RoutedUICommand SaveFile { get { return saveFile; } }
        public static RoutedUICommand SaveFileAs { get { return saveFileAs; } }
        public static RoutedUICommand CloseTab { get { return closeTab; } }
        public static RoutedUICommand CloseAllTab { get { return closeAllTab; } }
        public static RoutedUICommand StartCapture { get { return startCapturePackets; } }
        public static RoutedUICommand StopCapture { get { return stopCapturePackets; } }
        public static RoutedUICommand WinPCap { get { return winpcap; } }
        public static RoutedUICommand Ekinar { get { return ekinar; } }
        public static RoutedUICommand OpenScript { get { return openScript; } }
        public static RoutedUICommand EditScript { get { return editScript; } }
        public static RoutedUICommand RunScript { get { return runScript; } }
        public static RoutedUICommand OpenTemplate { get { return openTemplate; } }
        public static RoutedUICommand EditTemplate { get { return editTemplate; } }
        public static RoutedUICommand RunTemplate { get { return runTemplate; } }
        public static RoutedUICommand ExitApplication { get { return exitApplication; } }
    }
}