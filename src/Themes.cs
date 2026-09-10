using System;
using System.Windows.Media;

namespace CountdownWidget
{
    public class WidgetTheme
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Brush BackgroundBrush { get; set; }
        public Brush BorderBrush { get; set; }
        public Brush AccentBrush { get; set; }
        public Brush TextPrimaryBrush { get; set; }
        public Brush TextSecondaryBrush { get; set; }
    }

    public static class ThemeManager
    {
        public static WidgetTheme GetDefaultTheme()
        {
            return new WidgetTheme
            {
                Name = "DarkGrey",
                DisplayName = "Dark Grey & White",
                BackgroundBrush = new SolidColorBrush(Color.FromArgb(235, 34, 34, 38)), // Sleek Dark Grey #222226
                BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),   // Subtle rim
                AccentBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),        // Pure White
                TextPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),   // Pure White
                TextSecondaryBrush = new SolidColorBrush(Color.FromRgb(212, 212, 216))  // Soft Light Grey #D4D4D8
            };
        }
    }
}
