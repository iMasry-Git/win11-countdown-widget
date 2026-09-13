using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace CountdownWidget
{
    public class WidgetConfig
    {
        public string EventName { get; set; }
        public DateTime TargetDate { get; set; }
        public double WindowX { get; set; }
        public double WindowY { get; set; }
        public bool IsLocked { get; set; }
        public bool StartWithWindows { get; set; }
        public bool IsFirstRun { get; set; }

        public WidgetConfig()
        {
            EventName = "New Year's Eve";
            int nextYear = DateTime.Today.Year + 1;
            TargetDate = new DateTime(nextYear, 1, 1);
            WindowX = double.NaN;
            WindowY = double.NaN;
            IsLocked = false;
            StartWithWindows = false;
            IsFirstRun = true;
        }
    }

    /// <summary>
    /// Intermediate DTO for JSON serialization.
    /// JSON has no NaN literal, so WindowX/WindowY use a sentinel value (-99999).
    /// TargetDate is stored as an ISO "yyyy-MM-dd" string for clarity.
    /// </summary>
    internal class ConfigDto
    {
        public string EventName { get; set; }
        public string TargetDate { get; set; }
        public double WindowX { get; set; }
        public double WindowY { get; set; }
        public bool IsLocked { get; set; }
        public bool StartWithWindows { get; set; }
        public bool IsFirstRun { get; set; }
    }

    public static class ConfigManager
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CountdownWidget"
        );

        private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        private const double NAN_SENTINEL = -99999.0;

        public static WidgetConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFile))
                {
                    var fresh = new WidgetConfig();
                    Save(fresh);
                    return fresh;
                }

                string json = File.ReadAllText(ConfigFile, Encoding.UTF8);
                return FromDto(_serializer.Deserialize<ConfigDto>(json));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading config: " + ex.Message);
                return new WidgetConfig();
            }
        }

        public static void Save(WidgetConfig config)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);
                }

                string json = _serializer.Serialize(ToDto(config));
                File.WriteAllText(ConfigFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving config: " + ex.Message);
            }
        }

        private static ConfigDto ToDto(WidgetConfig c)
        {
            return new ConfigDto
            {
                EventName = c.EventName ?? "",
                TargetDate = c.TargetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                WindowX = double.IsNaN(c.WindowX) ? NAN_SENTINEL : c.WindowX,
                WindowY = double.IsNaN(c.WindowY) ? NAN_SENTINEL : c.WindowY,
                IsLocked = c.IsLocked,
                StartWithWindows = c.StartWithWindows,
                IsFirstRun = c.IsFirstRun
            };
        }

        private static WidgetConfig FromDto(ConfigDto d)
        {
            var c = new WidgetConfig();

            if (d == null) return c;

            c.EventName = d.EventName ?? c.EventName;

            DateTime td;
            if (!string.IsNullOrEmpty(d.TargetDate) &&
                (DateTime.TryParse(d.TargetDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out td) ||
                 DateTime.TryParse(d.TargetDate, out td)))
            {
                c.TargetDate = td.Date;
            }

            c.WindowX = (d.WindowX <= -90000) ? double.NaN : d.WindowX;
            c.WindowY = (d.WindowY <= -90000) ? double.NaN : d.WindowY;
            c.IsLocked = d.IsLocked;
            c.StartWithWindows = d.StartWithWindows;
            c.IsFirstRun = d.IsFirstRun;

            return c;
        }
    }
}
