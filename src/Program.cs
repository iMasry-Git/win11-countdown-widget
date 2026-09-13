using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using Forms = System.Windows.Forms;

namespace CountdownWidget
{
    public class Program
    {
        private static Mutex _appMutex;
        private static Forms.NotifyIcon _notifyIcon;
        private static WidgetWindow _widgetWindow;
        private static WidgetConfig _config;

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Logger.LogError("Unhandled AppDomain", e.ExceptionObject as Exception);
            };

            bool isFirstInstance = true;
            try
            {
                _appMutex = new Mutex(true, "Local\\Win11CountdownWidget_AppMutex", out isFirstInstance);
            }
            catch (Exception ex)
            {
                Logger.LogError("Mutex acquisition failed", ex);
                isFirstInstance = true;
            }

            if (!isFirstInstance)
            {
                Forms.MessageBox.Show(
                    "Countdown Widget is already running! Look for the widget on your desktop or in the system tray.",
                    "Countdown Widget",
                    Forms.MessageBoxButtons.OK,
                    Forms.MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                var app = new Application();
                app.DispatcherUnhandledException += (s, e) =>
                {
                    Logger.LogError("Dispatcher exception", e.Exception);
                    e.Handled = true;
                };

                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _config = ConfigManager.Load();

                _widgetWindow = new WidgetWindow(_config);
                app.MainWindow = _widgetWindow;

                SetupTrayIcon();

                _widgetWindow.Show();

                if (_config.IsFirstRun)
                {
                    _widgetWindow.OpenSettings();
                }

                app.Run();
            }
            catch (Exception ex)
            {
                Logger.LogError("Main execution failed", ex);
            }
            finally
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }

                if (_appMutex != null && isFirstInstance)
                {
                    try { _appMutex.ReleaseMutex(); } catch {}
                    _appMutex.Dispose();
                }
            }
        }

        private static void SetupTrayIcon()
        {
            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Icon = GenerateTrayIcon();
            _notifyIcon.Text = "Countdown Widget: " + TruncateString(_config.EventName, 40);
            _notifyIcon.Visible = true;

            var contextMenu = new Forms.ContextMenuStrip();

            var itemOpen = new Forms.ToolStripMenuItem("Show Widget");
            itemOpen.Click += (s, e) =>
            {
                if (_widgetWindow != null)
                {
                    _widgetWindow.Show();
                    _widgetWindow.WindowState = WindowState.Normal;
                    _widgetWindow.Activate();
                }
            };
            contextMenu.Items.Add(itemOpen);

            var itemSettings = new Forms.ToolStripMenuItem("Edit Event & Date...");
            itemSettings.Click += (s, e) =>
            {
                if (_widgetWindow != null)
                {
                    _widgetWindow.Show();
                    _widgetWindow.Activate();
                    _widgetWindow.OpenSettings();
                }
            };
            contextMenu.Items.Add(itemSettings);

            contextMenu.Items.Add(new Forms.ToolStripSeparator());

            var itemExit = new Forms.ToolStripMenuItem("Exit");
            itemExit.Click += (s, e) =>
            {
                _notifyIcon.Visible = false;
                if (_widgetWindow != null)
                {
                    _widgetWindow.CleanupAndExit();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
            contextMenu.Items.Add(itemExit);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (_widgetWindow != null)
                {
                    _widgetWindow.Show();
                    _widgetWindow.WindowState = WindowState.Normal;
                    _widgetWindow.Activate();
                }
            };
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static Icon GenerateTrayIcon()
        {
            using (var bmp = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(255, 108, 0)))
                {
                    g.FillEllipse(brush, 1, 1, 30, 30);
                }

                using (var pen = new Pen(System.Drawing.Color.White, 2.5f))
                {
                    g.DrawLine(pen, 16, 16, 16, 7);
                    g.DrawLine(pen, 16, 16, 23, 16);
                }

                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    // Clone the icon so the managed copy owns its own resources,
                    // then destroy the unmanaged handle to prevent a GDI leak.
                    using (var tempIcon = Icon.FromHandle(hIcon))
                    {
                        return (Icon)tempIcon.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }

        private static string TruncateString(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "Countdown Widget";
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen - 3) + "...";
        }
    }
}
