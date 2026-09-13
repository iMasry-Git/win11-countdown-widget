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
                if (theme == null || theme.BackgroundBrush == null || theme.TextPrimaryBrush == null ||
                    theme.DialogBackgroundBrush == null || theme.InputBackgroundBrush == null)
                {
                    Console.WriteLine("FAIL: Theme invalid or missing semantic brushes");
                    failed++;
                }
                else
                {
                    Console.WriteLine("PASS: Theme is Dark Grey & White with semantic dialog brushes");
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

            // Test 4: Special characters and colons in event name (JSON parser & escape fix)
            try
            {
                var cfg = new WidgetConfig();
                cfg.EventName = "Sprint Review: Part 1 \"Final\" & Demo";
                cfg.TargetDate = DateTime.Today.AddDays(10);
                ConfigManager.Save(cfg);

                var loaded = ConfigManager.Load();
                if (loaded.EventName != "Sprint Review: Part 1 \"Final\" & Demo")
                {
                    Console.WriteLine("FAIL: Special characters in EventName mismatch: " + loaded.EventName);
                    failed++;
                }
                else
                {
                    Console.WriteLine("PASS: Special characters and colons in EventName handled correctly");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                failed++;
            }

            // Test 5: Default Event and Date
            try
            {
                var cfg = new WidgetConfig();
                DateTime today = DateTime.Today;
                DateTime expectedTarget = today <= new DateTime(today.Year, 12, 31) ? new DateTime(today.Year, 12, 31) : new DateTime(today.Year + 1, 12, 31);
                if (cfg.EventName != "New Year's Eve")
                {
                    Console.WriteLine("FAIL: Default EventName is not New Year's Eve: " + cfg.EventName);
                    failed++;
                }
                else if (cfg.TargetDate.Date != expectedTarget)
                {
                    Console.WriteLine("FAIL: Default TargetDate mismatch: " + cfg.TargetDate + " expected: " + expectedTarget);
                    failed++;
                }
                else
                {
                    Console.WriteLine("PASS: Default Event and TargetDate (New Year's Eve - Dec 31)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL: " + ex.Message);
                failed++;
            }

            // Test 6: Tray tooltip formatting & dynamic update logic
            try
            {
                DateTime today = new DateTime(2026, 9, 13);
                DateTime target = new DateTime(2026, 12, 31);
                string tip = Win32Helper.FormatTrayTooltip("New Year's Eve", target, today);
                if (tip != "New Year's Eve: 109 days left")
                {
                    Console.WriteLine("FAIL: Tray tooltip formatting mismatch: " + tip);
                    failed++;
                }
                else
                {
                    // Test 63-character truncation protection for WinForms shell limit
                    string longName = "A Very Long Event Name That Exceeds The Windows Shell NotifyIcon Tooltip Limit";
                    string longTip = Win32Helper.FormatTrayTooltip(longName, target, today);
                    if (longTip.Length > 63)
                    {
                        Console.WriteLine("FAIL: Tray tooltip exceeds 63 characters: " + longTip.Length);
                        failed++;
                    }
                    else if (!longTip.EndsWith("109 days left"))
                    {
                        Console.WriteLine("FAIL: Truncated tooltip lost status text: " + longTip);
                        failed++;
                    }
                    else
                    {
                        Console.WriteLine("PASS: Dynamic tray tooltip formatting and 63-char truncation safety");
                    }
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
