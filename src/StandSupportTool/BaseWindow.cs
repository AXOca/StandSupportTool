// BaseWindow.cs
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StandSupportTool
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            Loaded += BaseWindow_Loaded;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableImmersiveDarkMode();
        }

        private void EnableImmersiveDarkMode()
        {
            var windowHandle = new WindowInteropHelper(this).Handle;
            var attribute = DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;
            var useImmersiveDarkMode = true;

            if (windowHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("The window handle is not initialized.");
            }

            int result = DwmSetWindowAttribute(windowHandle, attribute, ref useImmersiveDarkMode, Marshal.SizeOf(typeof(bool)));

            if (result != 0)
            {
                throw new InvalidOperationException("Failed to set window attribute.");
            }
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref bool pvAttribute, int cbAttribute);

        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20
        }
    }
}
