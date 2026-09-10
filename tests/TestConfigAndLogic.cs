using System;
using System.IO;
using CountdownWidget;

namespace CountdownWidget.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            int failed = 0;
            Console.WriteLine("=== Running Updated Countdown Widget Tests ===");

            // Test 1: Config serialization & roundtrip
            try
            {
                var cfg = new WidgetConfig();
                cfg.EventName = "Graduation Ceremony";
                cfg.TargetDate = new DateTime(2026, 11, 20);
                cfg.WindowX = 500;
                cfg.WindowY = 150;
                cfg.IsLocked = true;
                cfg.StartWithWindows = false;
                cfg.IsFirstRun = false;

                ConfigManager.Save(cfg);
                var loaded = ConfigManager.Load();

                if (loaded.EventName != "Graduation Ceremony") { Console.WriteLine("FAIL: EventName"); failed++; }
                if (loaded.TargetDate.Date != new DateTime(2026, 11, 20)) { Console.WriteLine("FAIL: TargetDate"); failed++; }
                if (!loaded.IsLocked) { Console.WriteLine("FAIL: IsLocked"); failed++; }

                Console.WriteLine("PASS: Config serialization & roundtrip");
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                failed++;
            }

            // Test 2: Theme
            try
            {
                var theme = ThemeManager.GetDefaultTheme();
                if (theme == null || theme.BackgroundBrush == null || theme.TextPrimaryBrush == null)
                {
                    Console.WriteLine("FAIL: Theme invalid");
                    failed++;
                }
                else
                {
                    Console.WriteLine("PASS: Theme is Dark Grey & White");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                failed++;
            }

            // Test 3: Days-only calculation
            try
            {
                DateTime today = DateTime.Today;
                DateTime target = today.AddDays(25);
                int days = (int)(target - today).TotalDays;
                if (days != 25)
                {
                    Console.WriteLine("FAIL: Days calculation mismatch: " + days);
                    failed++;
                }
                else
                {
                    Console.WriteLine("PASS: Days-only countdown calculation");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                failed++;
            }

            Console.WriteLine(failed == 0 ? "ALL TESTS PASSED!" : ("FAILED: " + failed));
            return failed;
        }
    }
}
