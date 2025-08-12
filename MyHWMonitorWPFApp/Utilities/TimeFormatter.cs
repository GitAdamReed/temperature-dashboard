using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHWMonitorWPFApp.Utilities
{
    public static class TimeFormatter
    {
        public static string FormatTotalSeconds(double totalSeconds)
        {
            StringBuilder sb = new(string.Empty);
            // Convert to hours
            if (Math.Floor(totalSeconds / 3600) > 0) sb.Append($"{Math.Floor(totalSeconds / 3600)}h");
            // Convert to minutes but omit if multiple of 3600
            if (Math.Floor(totalSeconds / 60) > 0 && Math.Floor(totalSeconds % 3600) != 0) sb.Append($"{Math.Floor(totalSeconds / 60)}m");
            // Convert to seconds but omit if multiple of 60
            if (Math.Floor(totalSeconds % 60) != 0) sb.Append($"{(totalSeconds % 60):F0}s");

            string formattedString = sb.ToString();
            return formattedString == string.Empty ? $"{totalSeconds:F0}s" : formattedString;
        }
    }
}
