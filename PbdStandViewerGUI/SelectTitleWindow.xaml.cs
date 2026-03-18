using System;
using System.Windows;
using System.Windows.Interop;
using PbdStandViewerGUI.Win32Native;

namespace PbdStandViewerGUI
{
    /// <summary>
    /// SelectTitleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectTitleWindow : Window
    {
        public SelectTitleWindow()
        {
            InitializeComponent();
            this.DataContext = SelectTitleWindowViewModel.Instance;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            //禁用图标
            long exStyle = (long)User32.GetWindowLong(hwnd, User32.GWL_EXSTYLE);
            User32.SetWindowLong(hwnd, User32.GWL_EXSTYLE, new IntPtr(exStyle | User32.WS_EX_DLGMODALFRAME));
            User32.SendMessage(hwnd, User32.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            User32.SendMessage(hwnd, User32.WM_SETICON, new IntPtr(1), IntPtr.Zero);

            User32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_NOZORDER | User32.SWP_FRAMECHANGED);
        }
    }
}
