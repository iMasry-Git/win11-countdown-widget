using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Input;

namespace CountdownWidget
{
    public class SettingsWindow : Window
    {
        private WidgetConfig _config;
        private TextBox _txtEvent;
        private DatePicker _dpDate;
        private CheckBox _chkStartup;
        private TextBlock _lblPreview;

        public bool IsSaved { get; private set; }

        public SettingsWindow(WidgetConfig config)
        {
            _config = config;
            InitializeComponent();
            PopulateValues();
        }

        private void InitializeComponent()
        {
            Title = "Event Countdown Settings";
            Width = 360;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            var outerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 34)), // Dark Grey
                BorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(20, 16, 20, 16),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 24,
                    ShadowDepth = 5,
                    Opacity = 0.55
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Form
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Preview
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            // --- HEADER ---
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal };
            var iconText = new TextBlock
            {
                Text = "📅",
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var titleText = new TextBlock
            {
                Text = "Event Countdown",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            titlePanel.Children.Add(iconText);
            titlePanel.Children.Add(titleText);
            Grid.SetColumn(titlePanel, 0);

            var closeBtn = new Button
            {
                Content = "✕",
                Width = 26,
                Height = 26,
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 190)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                Cursor = Cursors.Hand
            };
            closeBtn.Click += (s, e) => Close();
            Grid.SetColumn(closeBtn, 1);

            headerGrid.Children.Add(titlePanel);
            headerGrid.Children.Add(closeBtn);
            Grid.SetRow(headerGrid, 0);

            headerGrid.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };

            // --- FORM ---
            var formStack = new StackPanel { Margin = new Thickness(0, 14, 0, 12) };

            // Event Name
            formStack.Children.Add(CreateLabel("1. Event Name:"));
            _txtEvent = new TextBox
            {
                FontSize = 13,
                Padding = new Thickness(8, 6, 8, 6),
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 48)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 4, 0, 10)
            };
            _txtEvent.TextChanged += (s, e) => UpdatePreview();
            formStack.Children.Add(_txtEvent);

            // Target Date
            formStack.Children.Add(CreateLabel("2. Target Date:"));
            _dpDate = new DatePicker
            {
                FontSize = 13,
                Margin = new Thickness(0, 4, 0, 8),
                SelectedDateFormat = DatePickerFormat.Long
            };
            _dpDate.SelectedDateChanged += (s, e) => UpdatePreview();
            formStack.Children.Add(_dpDate);

            // Quick Presets
            var presetWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
            presetWrap.Children.Add(CreatePresetButton("+7 Days", () => AddDays(7)));
            presetWrap.Children.Add(CreatePresetButton("+30 Days", () => AddDays(30)));
            presetWrap.Children.Add(CreatePresetButton("+100 Days", () => AddDays(100)));
            presetWrap.Children.Add(CreatePresetButton("New Year", () => SetToNewYear()));
            formStack.Children.Add(presetWrap);

            _chkStartup = new CheckBox
            {
                Content = "Start automatically with Windows",
                Foreground = new SolidColorBrush(Color.FromRgb(210, 210, 220)),
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0)
            };
            formStack.Children.Add(_chkStartup);

            Grid.SetRow(formStack, 1);

            // --- LIVE PREVIEW BOX ---
            var previewBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(100, 20, 20, 24)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 0, 0, 12)
            };
            _lblPreview = new TextBlock
            {
                Text = "Calculating...",
                Foreground = Brushes.White,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
            previewBorder.Child = _lblPreview;
            Grid.SetRow(previewBorder, 2);

            // --- BUTTONS ---
            var btnGrid = new Grid();
            btnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            btnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Height = 32,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 56)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Cursor = Cursors.Hand
            };
            cancelBtn.Click += (s, e) => Close();
            Grid.SetColumn(cancelBtn, 0);

            var saveBtn = new Button
            {
                Content = "Save & Apply",
                Height = 32,
                Margin = new Thickness(5, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Cursor = Cursors.Hand
            };
            saveBtn.Click += SaveAndClose;
            Grid.SetColumn(saveBtn, 1);

            btnGrid.Children.Add(cancelBtn);
            btnGrid.Children.Add(saveBtn);
            Grid.SetRow(btnGrid, 3);

            mainGrid.Children.Add(headerGrid);
            mainGrid.Children.Add(formStack);
            mainGrid.Children.Add(previewBorder);
            mainGrid.Children.Add(btnGrid);

            outerBorder.Child = mainGrid;
            Content = outerBorder;
        }

        private TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 225)),
                FontSize = 12,
                FontWeight = FontWeights.Medium
            };
        }

        private Button CreatePresetButton(string label, Action onClick)
        {
            var btn = new Button
            {
                Content = label,
                Padding = new Thickness(7, 3, 7, 3),
                Margin = new Thickness(0, 0, 6, 2),
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void AddDays(int days)
        {
            _dpDate.SelectedDate = DateTime.Today.AddDays(days);
            UpdatePreview();
        }

        private void SetToNewYear()
        {
            int nextYear = DateTime.Today.Year + 1;
            _dpDate.SelectedDate = new DateTime(nextYear, 1, 1);
            UpdatePreview();
        }

        private void PopulateValues()
        {
            _txtEvent.Text = _config.EventName;
            _dpDate.SelectedDate = _config.TargetDate.Date;
            _chkStartup.IsChecked = _config.StartWithWindows;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_lblPreview == null || _dpDate == null || _txtEvent == null) return;

            string eventName = string.IsNullOrWhiteSpace(_txtEvent.Text) ? "Event" : _txtEvent.Text.Trim();
            DateTime target = _dpDate.SelectedDate ?? DateTime.Today.AddDays(1);
            int days = (int)(target.Date - DateTime.Today).TotalDays;

            if (days > 0)
            {
                _lblPreview.Text = string.Format("🎯 {0} Day{1} Left until \"{2}\"\n({3:ddd, MMM d, yyyy})",
                    days, days == 1 ? "" : "s", eventName, target);
            }
            else if (days == 0)
            {
                _lblPreview.Text = string.Format("🎉 \"{0}\" is TODAY!", eventName);
            }
            else
            {
                int past = Math.Abs(days);
                _lblPreview.Text = string.Format("⚠️ Passed {0} day{1} ago ({2:ddd, MMM d, yyyy})",
                    past, past == 1 ? "" : "s", target);
            }
        }

        private void SaveAndClose(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtEvent.Text))
            {
                MessageBox.Show("Please enter an event name.", "Missing Event Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _config.EventName = _txtEvent.Text.Trim();
            _config.TargetDate = (_dpDate.SelectedDate ?? DateTime.Today.AddDays(1)).Date;
            _config.IsFirstRun = false;
            _config.StartWithWindows = _chkStartup.IsChecked == true;

            Win32Helper.SetStartup(_config.StartWithWindows);
            ConfigManager.Save(_config);

            IsSaved = true;
            Close();
        }
    }
}
