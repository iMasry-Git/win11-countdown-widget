using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CountdownWidget
{
    public static class Win32Helper
    {
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;

        public const int WM_WINDOWPOSCHANGING = 0x0046;

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static void SendToBottom(IntPtr hwnd)
        {
            try
            {
                if (hwnd != IntPtr.Zero)
                {
                    SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
            }
            catch {}
        }

        public static void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        if (enable)
                        {
                            key.SetValue("CountdownWidget", "\"" + appPath + "\"");
                        }
                        else
                        {
                            key.DeleteValue("CountdownWidget", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error configuring startup registry: " + ex.Message);
            }
        }

        public static string FormatTrayTooltip(string eventName, DateTime targetDate, DateTime today)
        {
            int days = (int)(targetDate.Date - today.Date).TotalDays;

            string statusText;
            if (days > 0)
            {
                statusText = string.Format("{0} day{1} left", days, days == 1 ? "" : "s");
            }
            else if (days == 0)
            {
                statusText = "Today!";
            }
            else
            {
                int past = Math.Abs(days);
                statusText = string.Format("{0} day{1} ago", past, past == 1 ? "" : "s");
            }

            string name = !string.IsNullOrEmpty(eventName) ? eventName : "Countdown Widget";
            string fullText = string.Format("{0}: {1}", name, statusText);

            // Windows Shell NotifyIcon.Text has a 63-character limit in .NET Framework WinForms
            if (fullText.Length > 63)
            {
                int maxEventLen = 63 - statusText.Length - 5; // leave room for "...: "
                if (maxEventLen > 3)
                {
                    string truncatedEvent = name.Substring(0, maxEventLen).TrimEnd() + "...";
                    fullText = string.Format("{0}: {1}", truncatedEvent, statusText);
                }
                else
                {
                    fullText = fullText.Substring(0, 63);
                }
            }

            return fullText;
        }
    }
}
