using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace CountdownWidget
{
    public class WidgetWindow : Window
    {
        private WidgetConfig _config;
        private WidgetTheme _theme;
        private DispatcherTimer _timer;
        private DispatcherTimer _saveDebounceTimer;
        private IntPtr _hwnd = IntPtr.Zero;

        private Border _rootBorder;
        private TextBlock _txtEventName;
        private TextBlock _txtDaysNumber;
        private TextBlock _txtDaysUnit;
        private TextBlock _txtTargetDate;
        private Button _btnLock;

        public WidgetWindow(WidgetConfig config)
        {
            _config = config;
            _theme = ThemeManager.GetDefaultTheme();
            InitializeComponent();
            RestorePosition();
            UpdateCountdown();

            // Refresh countdown periodically
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += (s, e) => UpdateCountdown();
            _timer.Start();

            // Debounce position saves: wait 500ms after the last move before writing to disk.
            // This prevents hundreds of disk writes during a single DragMove() operation.
            _saveDebounceTimer = new DispatcherTimer();
            _saveDebounceTimer.Interval = TimeSpan.FromMilliseconds(500);
            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                FlushPositionToDisk();
            };

            SourceInitialized += OnSourceInitialized;
            Activated += (s, e) => EnsureDesktopLayer();
        }

        private void InitializeComponent()
        {
            Title = "Desktop Countdown Widget";
            Width = 230;
            Height = 105;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = false;
            ResizeMode = ResizeMode.NoResize;

            _rootBorder = new Border
            {
                Background = _theme.BackgroundBrush,
                BorderBrush = _theme.BorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14, 10, 14, 10),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 16,
                    ShadowDepth = 3,
                    Opacity = 0.35
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Countdown body

            // --- HEADER ---
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var eventPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var icon = new TextBlock
            {
                Text = "📅",
                FontSize = 11,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _txtEventName = new TextBlock
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.TextPrimaryBrush,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 145,
                VerticalAlignment = VerticalAlignment.Center
            };
            eventPanel.Children.Add(icon);
            eventPanel.Children.Add(_txtEventName);
            Grid.SetColumn(eventPanel, 0);

            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal };

            _btnLock = CreateIconButton(_config.IsLocked ? "🔒" : "🔓", "Toggle Drag Lock", (s, e) => ToggleLock());
            actionsPanel.Children.Add(_btnLock);

            var btnEdit = CreateIconButton("⚙️", "Edit Event & Date", (s, e) => OpenSettings());
            actionsPanel.Children.Add(btnEdit);

            var btnClose = CreateIconButton("✕", "Close to Tray", (s, e) => Close());
            actionsPanel.Children.Add(btnClose);

            Grid.SetColumn(actionsPanel, 1);

            headerGrid.Children.Add(eventPanel);
            headerGrid.Children.Add(actionsPanel);
            Grid.SetRow(headerGrid, 0);

            // --- COUNTDOWN BODY ---
            var countdownGrid = new Grid { VerticalAlignment = VerticalAlignment.Center };
            countdownGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            countdownGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _txtDaysNumber = new TextBlock
            {
                FontSize = 38,
                FontWeight = FontWeights.ExtraBold,
                Foreground = _theme.AccentBrush,
                Margin = new Thickness(0, -5, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_txtDaysNumber, 0);

            var metaPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _txtDaysUnit = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.TextPrimaryBrush,
                Margin = new Thickness(0, 0, 0, 1)
            };
            _txtTargetDate = new TextBlock
            {
                FontSize = 10,
                Foreground = _theme.TextSecondaryBrush,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            metaPanel.Children.Add(_txtDaysUnit);
            metaPanel.Children.Add(_txtTargetDate);
            Grid.SetColumn(metaPanel, 1);

            countdownGrid.Children.Add(_txtDaysNumber);
            countdownGrid.Children.Add(metaPanel);
            Grid.SetRow(countdownGrid, 1);

            mainGrid.Children.Add(headerGrid);
            mainGrid.Children.Add(countdownGrid);

            _rootBorder.Child = mainGrid;
            Content = _rootBorder;

            // Interactions
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseDoubleClick += (s, e) => OpenSettings();
            LocationChanged += (s, e) => SavePosition();

            ContextMenu = BuildContextMenu();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            if (_hwnd != IntPtr.Zero)
            {
                // Hook WndProc to keep window strictly at bottom of z-order
                HwndSource source = HwndSource.FromHwnd(_hwnd);
                if (source != null)
                {
                    source.AddHook(WndProc);
                }

                EnsureDesktopLayer();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_WINDOWPOSCHANGING)
            {
                // Force window to always stay at bottom of z-order (under all windows)
                Win32Helper.WINDOWPOS wp = (Win32Helper.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(Win32Helper.WINDOWPOS));
                wp.hwndInsertAfter = Win32Helper.HWND_BOTTOM;
                Marshal.StructureToPtr(wp, lParam, true);
            }
            return IntPtr.Zero;
        }

        private void EnsureDesktopLayer()
        {
            if (_hwnd != IntPtr.Zero)
            {
                Win32Helper.SendToBottom(_hwnd);
            }
        }

        private Button CreateIconButton(string icon, string tooltip, RoutedEventHandler onClick)
        {
            var btn = new Button
            {
                Content = icon,
                Width = 20,
                Height = 20,
                Margin = new Thickness(2, 0, 0, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                FontSize = 9,
                Cursor = Cursors.Hand,
                ToolTip = tooltip
            };
            btn.Click += onClick;
            return btn;
        }

        private ContextMenu BuildContextMenu()
        {
            var menu = new ContextMenu();

            var miEdit = new MenuItem { Header = "✏️ Edit Event & Date..." };
            miEdit.Click += (s, e) => OpenSettings();
            menu.Items.Add(miEdit);

            var miLock = new MenuItem
            {
                Header = "🔒 Lock Position",
                IsCheckable = true,
                IsChecked = _config.IsLocked
            };
            miLock.Click += (s, e) =>
            {
                _config.IsLocked = miLock.IsChecked;
                _btnLock.Content = _config.IsLocked ? "🔒" : "🔓";
                ConfigManager.Save(_config);
            };
            menu.Items.Add(miLock);

            var miStartup = new MenuItem
            {
                Header = "🚀 Start with Windows",
                IsCheckable = true,
                IsChecked = _config.StartWithWindows
            };
            miStartup.Click += (s, e) =>
            {
                _config.StartWithWindows = miStartup.IsChecked;
                Win32Helper.SetStartup(_config.StartWithWindows);
                ConfigManager.Save(_config);
            };
            menu.Items.Add(miStartup);

            menu.Items.Add(new Separator());

            var miExit = new MenuItem { Header = "❌ Exit Widget" };
            miExit.Click += (s, e) =>
            {
                _timer.Stop();
                Application.Current.Shutdown();
            };
            menu.Items.Add(miExit);

            return menu;
        }

        public void UpdateCountdown()
        {
            _txtEventName.Text = _config.EventName;
            DateTime target = _config.TargetDate.Date;
            DateTime today = DateTime.Today;
            int days = (int)(target - today).TotalDays;

            _txtTargetDate.Text = target.ToString("ddd, MMM d, yyyy");

            if (days > 0)
            {
                _txtDaysNumber.Text = days.ToString();
                _txtDaysUnit.Text = days == 1 ? "DAY LEFT" : "DAYS LEFT";
            }
            else if (days == 0)
            {
                _txtDaysNumber.Text = "🎉";
                _txtDaysUnit.Text = "TODAY!";
            }
            else
            {
                int past = Math.Abs(days);
                _txtDaysNumber.Text = past.ToString();
                _txtDaysUnit.Text = past == 1 ? "DAY AGO" : "DAYS AGO";
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_config.IsLocked && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
                EnsureDesktopLayer();
            }
        }

        private void ToggleLock()
        {
            _config.IsLocked = !_config.IsLocked;
            _btnLock.Content = _config.IsLocked ? "🔒" : "🔓";
            ConfigManager.Save(_config);
            ContextMenu = BuildContextMenu();
        }

        private void RestorePosition()
        {
            if (!double.IsNaN(_config.WindowX) && !double.IsNaN(_config.WindowY))
            {
                Left = _config.WindowX;
                Top = _config.WindowY;
            }
            else
            {
                Rect workArea = SystemParameters.WorkArea;
                Left = workArea.Right - Width - 30;
                Top = workArea.Top + 50;
            }
        }

        private void SavePosition()
        {
            if (WindowState == WindowState.Normal)
            {
                // Update in-memory config immediately (cheap)
                _config.WindowX = Left;
                _config.WindowY = Top;

                // Restart the debounce timer — only writes to disk once
                // 500ms after the last position change (end of drag).
                _saveDebounceTimer.Stop();
                _saveDebounceTimer.Start();
            }
        }

        private void FlushPositionToDisk()
        {
            ConfigManager.Save(_config);
        }

        public void OpenSettings()
        {
            var settings = new SettingsWindow(_config);
            settings.Owner = this;
            settings.ShowDialog();

            if (settings.IsSaved)
            {
                UpdateCountdown();
                ContextMenu = BuildContextMenu();
            }
            EnsureDesktopLayer();
        }
    }
}
