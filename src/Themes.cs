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
        public Brush DividerBrush { get; set; }
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
                Name = "DarkOrange",
                DisplayName = "Dark Charcoal & Vivid Orange",
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(34, 35, 38)),        // Dark Slate #222326
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),   // Subtle rim
                DividerBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),  // 1px vertical divider
                AccentBrush = new SolidColorBrush(Color.FromRgb(255, 108, 0)),           // Vivid Electric Orange #FF6C00
                TextPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),   // Pure White
                TextSecondaryBrush = new SolidColorBrush(Color.FromRgb(165, 170, 182)), // Soft Light Grey #A5AAB6
                TextMutedBrush = new SolidColorBrush(Color.FromRgb(130, 134, 146)),     // Muted Slate Grey

                DialogBackgroundBrush = new SolidColorBrush(Color.FromRgb(30, 31, 35)),
                DialogBorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                InputBackgroundBrush = new SolidColorBrush(Color.FromRgb(40, 42, 48)),
                ButtonBackgroundBrush = new SolidColorBrush(Color.FromRgb(50, 52, 60)),
                ButtonPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 108, 0)),   // Vivid Orange primary button
                SurfaceSubtleBrush = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
                PreviewBackgroundBrush = new SolidColorBrush(Color.FromArgb(120, 20, 21, 24))
            };
        }
    }
}
