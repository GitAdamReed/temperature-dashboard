using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyHWMonitorWPFApp.Utilities
{
    public static class CustomBrush
    {
        // Generate a new unique SolidColorBrush with a random RGB colour
        public static SolidColorBrush RandomBrushUnique(List<SolidColorBrush> currentBrushes)
        {
            SolidColorBrush brush;

            // Keep generating new brushes until the brush colour is unique
            do
            {
                byte r = (byte)Random.Shared.Next(0, 256);
                byte g = (byte)Random.Shared.Next(0, 256);
                byte b = (byte)Random.Shared.Next(0, 256);
                brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            while (currentBrushes.Any(b => b.Color == brush.Color));

            currentBrushes.Add(brush);
            return brush;
        }
    }
}
