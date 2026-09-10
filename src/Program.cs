using System;
using System.IO;
using System.Threading;
using System.Drawing;
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
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CountdownWidget");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            string logFile = Path.Combine(appData, "widget.log");

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                File.AppendAllText(logFile, "Unhandled AppDomain: " + e.ExceptionObject.ToString() + Environment.NewLine);
            };

            bool isFirstInstance = true;
            try
            {
                _appMutex = new Mutex(true, "Local\\Win11CountdownWidget_AppMutex", out isFirstInstance);
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, "Mutex exception: " + ex.Message + Environment.NewLine);
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
                    File.AppendAllText(logFile, "Dispatcher exception: " + e.Exception.ToString() + Environment.NewLine);
                    e.Handled = true;
                };

                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _config = ConfigManager.Load();
                File.AppendAllText(logFile, "Config loaded. Event: " + _config.EventName + Environment.NewLine);

                _widgetWindow = new WidgetWindow(_config);
                app.MainWindow = _widgetWindow;
                File.AppendAllText(logFile, "WidgetWindow created" + Environment.NewLine);

                SetupTrayIcon();
                File.AppendAllText(logFile, "TrayIcon setup complete" + Environment.NewLine);

                _widgetWindow.Show();
                File.AppendAllText(logFile, "WidgetWindow shown" + Environment.NewLine);

                if (_config.IsFirstRun)
                {
                    File.AppendAllText(logFile, "Opening settings dialog" + Environment.NewLine);
                    _widgetWindow.OpenSettings();
                }

                File.AppendAllText(logFile, "Entering app.Run dispatcher loop..." + Environment.NewLine);
                app.Run();
                File.AppendAllText(logFile, "app.Run exited cleanly." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, "Main exception: " + ex.ToString() + Environment.NewLine);
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
                Application.Current.Shutdown();
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

        private static Icon GenerateTrayIcon()
        {
            using (var bmp = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(37, 99, 235)))
                {
                    g.FillEllipse(brush, 1, 1, 30, 30);
                }

                using (var pen = new Pen(System.Drawing.Color.White, 2.5f))
                {
                    g.DrawLine(pen, 16, 16, 16, 7);
                    g.DrawLine(pen, 16, 16, 23, 16);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
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
