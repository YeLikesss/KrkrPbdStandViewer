using System;
using System.Runtime.InteropServices;

namespace PbdStandViewerGUI.Win32Native
{
    /// <summary>
    /// User32 Dll
    /// </summary>
    internal class User32
    {
        const string DllName = "user32.dll";

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const uint WM_SETICON = 0x0080;

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW", ExactSpelling = false)]
        public static extern IntPtr GetWindowLong64(IntPtr hwnd, int index);
        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongW", ExactSpelling = false)]
        public static extern IntPtr GetWindowLong32(IntPtr hwnd, int index);

        public static IntPtr GetWindowLong(IntPtr hwnd, int index)
        {
            if (UIntPtr.Size == 8)
            {
                return GetWindowLong64(hwnd, index);
            }
            else
            {
                return GetWindowLong32(hwnd, index);
            }
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", ExactSpelling = false)]
        public static extern IntPtr SetWindowLong64(IntPtr hwnd, int index, IntPtr newStyle);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongW", ExactSpelling = false)]
        public static extern IntPtr SetWindowLong32(IntPtr hwnd, int index, IntPtr newStyle);

        public static IntPtr SetWindowLong(IntPtr hwnd, int index, IntPtr newStyle)
        {
            if (UIntPtr.Size == 8)
            {
                return SetWindowLong64(hwnd, index, newStyle);
            }
            else
            {
                return SetWindowLong32(hwnd, index, newStyle);
            }
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SetWindowPos", ExactSpelling = false)]
        [return:MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport(DllName, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", ExactSpelling = false)]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
