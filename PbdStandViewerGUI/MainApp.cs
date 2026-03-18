using System;
using System.Windows;

namespace PbdStandViewerGUI
{
    public class MainApp : Application
    {
        [STAThread]
        public static void Main()
        {
            PbdStandViewerGUI.MainApp app = new();
            app.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
            app.Run();
        }
    }
}
