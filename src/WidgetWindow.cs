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
        private readonly WidgetConfig _config;
        private readonly WidgetTheme _theme;
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _saveDebounceTimer;

        private IntPtr _hwnd = IntPtr.Zero;
        private HwndSource _hwndSource;

        private Border _rootBorder;
        private TextBlock _txtDaysNumber;
        private TextBlock _txtEventName;
        private TextBlock _txtTargetDate;
        private TextBlock _txtDaysLine1;
        private TextBlock _txtDaysLine2;
        private Button _btnLock;

        private ContextMenu _contextMenu;
        private MenuItem _miLock;
        private MenuItem _miStartup;

        public WidgetWindow(WidgetConfig config)
        {
            _config = config;
            _theme = ThemeManager.GetDefaultTheme();

            InitializeComponent();
            RestorePosition();
            UpdateCountdown();

            // Refresh countdown every minute to stay accurate across day boundaries
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += (s, e) => UpdateCountdown();
            _timer.Start();

            // Debounce position saves: wait 500ms after dragging stops before writing to disk
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
            Width = 225;
            Height = 128;
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
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 6, 12, 8),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 18,
                    ShadowDepth = 4,
                    Opacity = 0.55
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(16, GridUnitType.Pixel) }); // Row 0: Top actions
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });   // Row 1: Content body

            var fontIcons = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol");
            var fontBahnschrift = new FontFamily("Bahnschrift, Segoe UI, sans-serif");

            // --- TOP ACTIONS BAR ---
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 1, 0)
            };

            _btnLock = CreateIconButton(_config.IsLocked ? "\uE72E" : "\uE785", "Toggle Drag Lock", (s, e) => ToggleLock());
            actionsPanel.Children.Add(_btnLock);

            var btnEdit = CreateIconButton("\uE713", "Edit Event & Date", (s, e) => OpenSettings());
            actionsPanel.Children.Add(btnEdit);

            var btnClose = CreateIconButton("\uE8BB", "Close to Tray", (s, e) => Hide());
            actionsPanel.Children.Add(btnClose);

            Grid.SetRow(actionsPanel, 0);
            mainGrid.Children.Add(actionsPanel);

            // --- CONTENT BODY: LEFT (NUMBER) | DIVIDER | RIGHT (EVENT & DAYS LEFT) ---
            var bodyGrid = new Grid();
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(86, GridUnitType.Pixel) });
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Huge Orange Number
            _txtDaysNumber = new TextBlock
            {
                FontFamily = fontBahnschrift,
                FontSize = 74,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.AccentBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -3, 0, 0)
            };
            Grid.SetColumn(_txtDaysNumber, 0);
            bodyGrid.Children.Add(_txtDaysNumber);

            // Subtle Vertical Divider
            var divider = new Border
            {
                Width = 1,
                Background = _theme.DividerBrush,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(3, 1, 10, 1)
            };
            Grid.SetColumn(divider, 1);
            bodyGrid.Children.Add(divider);

            // Right Column: Top Info + Bottom DAYS LEFT
            var rightGrid = new Grid();
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Top Info: Event Title & Date
            var topInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 0, 0, 0) };

            _txtEventName = new TextBlock
            {
                FontFamily = fontBahnschrift,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.TextPrimaryBrush,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 1)
            };
            topInfo.Children.Add(_txtEventName);

            var datePanel = new StackPanel { Orientation = Orientation.Horizontal };
            var calIcon = new TextBlock
            {
                Text = "\uE787",
                FontFamily = fontIcons,
                FontSize = 9.5,
                Foreground = _theme.TextSecondaryBrush,
                Margin = new Thickness(0, 1, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _txtTargetDate = new TextBlock
            {
                FontFamily = fontBahnschrift,
                FontSize = 9.5,
                Foreground = _theme.TextSecondaryBrush,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            datePanel.Children.Add(calIcon);
            datePanel.Children.Add(_txtTargetDate);
            topInfo.Children.Add(datePanel);

            Grid.SetRow(topInfo, 0);
            rightGrid.Children.Add(topInfo);

            // Bottom Stacked Text: DAYS / LEFT
            var bottomText = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 0) };

            _txtDaysLine1 = new TextBlock
            {
                Text = "DAYS",
                FontFamily = fontBahnschrift,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.TextPrimaryBrush,
                Margin = new Thickness(0, 0, 0, -3)
            };
            _txtDaysLine2 = new TextBlock
            {
                Text = "LEFT",
                FontFamily = fontBahnschrift,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = _theme.TextPrimaryBrush,
                Margin = new Thickness(0, -3, 0, 0)
            };
            bottomText.Children.Add(_txtDaysLine1);
            bottomText.Children.Add(_txtDaysLine2);

            Grid.SetRow(bottomText, 1);
            rightGrid.Children.Add(bottomText);

            Grid.SetColumn(rightGrid, 2);
            bodyGrid.Children.Add(rightGrid);

            Grid.SetRow(bodyGrid, 1);
            mainGrid.Children.Add(bodyGrid);

            _rootBorder.Child = mainGrid;
            Content = _rootBorder;

            // Interactions
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseDoubleClick += (s, e) => OpenSettings();
            LocationChanged += (s, e) => SavePosition();

            InitializeContextMenu();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            if (_hwnd != IntPtr.Zero)
            {
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                if (_hwndSource != null)
                {
                    _hwndSource.AddHook(WndProc);
                }

                EnsureDesktopLayer();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_WINDOWPOSCHANGING)
            {
                // Force window to always stay at bottom of z-order (under all desktop windows)
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

        private Button CreateIconButton(string glyph, string tooltip, RoutedEventHandler onClick)
        {
            var btn = new Button
            {
                Content = new TextBlock
                {
                    Text = glyph,
                    FontFamily = new FontFamily("Segoe MDL2 Assets, Segoe UI Symbol"),
                    FontSize = 11,
                    Foreground = _theme.TextSecondaryBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 22,
                Height = 20,
                Margin = new Thickness(4, 0, 0, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = tooltip
            };

            btn.MouseEnter += (s, e) =>
            {
                btn.Background = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255));
                var tb = btn.Content as TextBlock;
                if (tb != null) tb.Foreground = _theme.TextPrimaryBrush;
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.Background = Brushes.Transparent;
                var tb = btn.Content as TextBlock;
                if (tb != null) tb.Foreground = _theme.TextSecondaryBrush;
            };

            btn.Click += onClick;
            return btn;
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenu();

            var miEdit = new MenuItem { Header = "✏️ Edit Event & Date..." };
            miEdit.Click += (s, e) => OpenSettings();
            _contextMenu.Items.Add(miEdit);

            _miLock = new MenuItem
            {
                Header = "🔒 Lock Position",
                IsCheckable = true,
                IsChecked = _config.IsLocked
            };
            _miLock.Click += (s, e) => ToggleLock();
            _contextMenu.Items.Add(_miLock);

            _miStartup = new MenuItem
            {
                Header = "🚀 Start with Windows",
                IsCheckable = true,
                IsChecked = _config.StartWithWindows
            };
            _miStartup.Click += (s, e) =>
            {
                _config.StartWithWindows = _miStartup.IsChecked;
                Win32Helper.SetStartup(_config.StartWithWindows);
                ConfigManager.Save(_config);
            };
            _contextMenu.Items.Add(_miStartup);

            _contextMenu.Items.Add(new Separator());

            var miExit = new MenuItem { Header = "❌ Exit Widget" };
            miExit.Click += (s, e) => CleanupAndExit();
            _contextMenu.Items.Add(miExit);

            ContextMenu = _contextMenu;
        }

        private void UpdateContextMenuState()
        {
            if (_miLock != null) _miLock.IsChecked = _config.IsLocked;
            if (_miStartup != null) _miStartup.IsChecked = _config.StartWithWindows;
        }

        public void UpdateCountdown()
        {
            _txtEventName.Text = _config.EventName;
            DateTime target = _config.TargetDate.Date;
            DateTime today = DateTime.Today;
            int days = (int)(target - today).TotalDays;

            _txtTargetDate.Text = target.ToString("ddd, MMM d, yyyy");

            int absDays = Math.Abs(days);

            // Dynamic number sizing for 1-2 digits, 3 digits, or 4+ digits
            if (absDays >= 1000)
            {
                _txtDaysNumber.FontSize = 35;
            }
            else if (absDays >= 100)
            {
                _txtDaysNumber.FontSize = 50;
            }
            else
            {
                _txtDaysNumber.FontSize = 74;
            }

            if (days > 0)
            {
                _txtDaysNumber.Text = days.ToString();
                _txtDaysLine1.Text = days == 1 ? "DAY" : "DAYS";
                _txtDaysLine2.Text = "LEFT";
            }
            else if (days == 0)
            {
                _txtDaysNumber.Text = "0";
                _txtDaysLine1.Text = "EVENT";
                _txtDaysLine2.Text = "TODAY!";
            }
            else
            {
                _txtDaysNumber.Text = absDays.ToString();
                _txtDaysLine1.Text = absDays == 1 ? "DAY" : "DAYS";
                _txtDaysLine2.Text = "AGO";
            }

            // Synchronize tray icon hover tooltip with updated event name and remaining days
            Program.UpdateTrayToolTip(_config.EventName, target);
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
            UpdateLockIcon();
            UpdateContextMenuState();
            ConfigManager.Save(_config);
        }

        private void UpdateLockIcon()
        {
            if (_btnLock != null)
            {
                var tb = _btnLock.Content as TextBlock;
                if (tb != null)
                {
                    tb.Text = _config.IsLocked ? "\uE72E" : "\uE785";
                }
            }
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
                _config.WindowX = Left;
                _config.WindowY = Top;

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
            var settings = new SettingsWindow(_config, _theme);
            settings.Owner = this;
            settings.ShowDialog();

            if (settings.IsSaved)
            {
                UpdateCountdown();
                UpdateLockIcon();
                UpdateContextMenuState();
            }
            EnsureDesktopLayer();
        }

        protected override void OnClosed(EventArgs e)
        {
            Cleanup();
            base.OnClosed(e);
        }

        private void Cleanup()
        {
            if (_saveDebounceTimer != null)
            {
                if (_saveDebounceTimer.IsEnabled)
                {
                    _saveDebounceTimer.Stop();
                    FlushPositionToDisk();
                }
            }

            if (_timer != null)
            {
                _timer.Stop();
            }

            if (_hwndSource != null)
            {
                try { _hwndSource.RemoveHook(WndProc); } catch {}
                _hwndSource = null;
            }
        }

        public void CleanupAndExit()
        {
            Cleanup();
            Application.Current.Shutdown();
        }
    }
}
