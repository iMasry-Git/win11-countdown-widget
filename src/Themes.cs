using System;
using System.Windows.Media;

namespace CountdownWidget
{
    public class WidgetTheme
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        // Widget Card Brushes
        public Brush BackgroundBrush { get; set; }
        public Brush BorderBrush { get; set; }
        public Brush AccentBrush { get; set; }
        public Brush TextPrimaryBrush { get; set; }
        public Brush TextSecondaryBrush { get; set; }
        public Brush TextMutedBrush { get; set; }

        // Settings / Dialog Brushes
        public Brush DialogBackgroundBrush { get; set; }
        public Brush DialogBorderBrush { get; set; }
        public Brush InputBackgroundBrush { get; set; }
        public Brush ButtonBackgroundBrush { get; set; }
        public Brush ButtonPrimaryBrush { get; set; }
        public Brush SurfaceSubtleBrush { get; set; }
        public Brush PreviewBackgroundBrush { get; set; }
    }

    public static class ThemeManager
    {
        public static WidgetTheme GetDefaultTheme()
        {
            return new WidgetTheme
            {
                Name = "DarkGrey",
                DisplayName = "Dark Grey & White",
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(235, 34, 34, 38)), // Frosted Dark Grey #222226
                BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),   // Subtle rim
                AccentBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),        // Pure White
                TextPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),   // Pure White
                TextSecondaryBrush = new SolidColorBrush(Color.FromRgb(212, 212, 216)), // Soft Light Grey #D4D4D8
                TextMutedBrush = new SolidColorBrush(Color.FromRgb(158, 158, 168)),     // Muted Grey

                DialogBackgroundBrush = new SolidColorBrush(Color.FromRgb(30, 30, 34)), // Dark Grey Dialog
                DialogBorderBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                InputBackgroundBrush = new SolidColorBrush(Color.FromRgb(42, 42, 48)),
                ButtonBackgroundBrush = new SolidColorBrush(Color.FromRgb(50, 50, 56)),
                ButtonPrimaryBrush = new SolidColorBrush(Color.FromRgb(70, 70, 80)),
                SurfaceSubtleBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                PreviewBackgroundBrush = new SolidColorBrush(Color.FromArgb(100, 20, 20, 24))
            };
        }
    }
}
