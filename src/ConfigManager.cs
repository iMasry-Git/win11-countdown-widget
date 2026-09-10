using System;
using System.IO;
using System.Text;
using System.Globalization;

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

    public static class ConfigManager
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CountdownWidget"
        );

        private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

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
                return ParseJson(json);
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

                string json = ToJson(config);
                File.WriteAllText(ConfigFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving config: " + ex.Message);
            }
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }

        private static string Unescape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        private static string ToJson(WidgetConfig c)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"EventName\": \"{0}\",", Escape(c.EventName)));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"TargetDate\": \"{0}\",", c.TargetDate.ToString("yyyy-MM-dd")));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"WindowX\": {0},", double.IsNaN(c.WindowX) ? -99999.0 : c.WindowX));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"WindowY\": {0},", double.IsNaN(c.WindowY) ? -99999.0 : c.WindowY));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"IsLocked\": {0},", c.IsLocked.ToString().ToLowerInvariant()));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"StartWithWindows\": {0},", c.StartWithWindows.ToString().ToLowerInvariant()));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  \"IsFirstRun\": {0}", c.IsFirstRun.ToString().ToLowerInvariant()));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static WidgetConfig ParseJson(string json)
        {
            var c = new WidgetConfig();
            string[] lines = json.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim().TrimEnd(',');
                int colonIdx = line.IndexOf(':');
                if (colonIdx <= 0) continue;

                string key = line.Substring(0, colonIdx).Trim().Trim('"');
                string val = line.Substring(colonIdx + 1).Trim();
                bool isString = val.StartsWith("\"") && val.EndsWith("\"");
                string cleanVal = isString ? val.Substring(1, val.Length - 2) : val;

                switch (key)
                {
                    case "EventName":
                        c.EventName = Unescape(cleanVal);
                        break;
                    case "TargetDate":
                        DateTime td;
                        if (DateTime.TryParse(cleanVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out td) ||
                            DateTime.TryParse(cleanVal, out td))
                        {
                            c.TargetDate = td.Date;
                        }
                        break;
                    case "WindowX":
                        double x;
                        if (double.TryParse(cleanVal, NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                            c.WindowX = (x <= -90000) ? double.NaN : x;
                        break;
                    case "WindowY":
                        double y;
                        if (double.TryParse(cleanVal, NumberStyles.Any, CultureInfo.InvariantCulture, out y))
                            c.WindowY = (y <= -90000) ? double.NaN : y;
                        break;
                    case "IsLocked":
                        bool l;
                        if (bool.TryParse(cleanVal, out l)) c.IsLocked = l;
                        break;
                    case "StartWithWindows":
                        bool sw;
                        if (bool.TryParse(cleanVal, out sw)) c.StartWithWindows = sw;
                        break;
                    case "IsFirstRun":
                        bool fr;
                        if (bool.TryParse(cleanVal, out fr)) c.IsFirstRun = fr;
                        break;
                }
            }
            return c;
        }
    }
}
