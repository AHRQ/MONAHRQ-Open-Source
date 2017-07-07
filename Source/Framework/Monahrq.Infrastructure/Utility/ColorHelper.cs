using System;
using System.Drawing;
using System.Windows.Forms;

namespace Monahrq.Infrastructure.Utility
{
    public static class ColorHelper
    {
        private const int N_THRESHOLD = 100; // 105;

        /// <summary>
        /// Darkens the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="percentDarken">The percent darken.</param>
        /// <returns></returns>
        public static Color Darken(Color color, float percentDarken)
        {
            return ControlPaint.Dark(color, percentDarken);
        }

        /// <summary>
        /// Lightens the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="percentLighten">The percent lighten.</param>
        /// <returns></returns>
        public static Color Lighten(Color color, float percentLighten)
        {
            return ControlPaint.Light(color, percentLighten); 
        }

        /// <summary>
        /// Gets the color of the ideal fore.
        /// </summary>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <returns></returns>
        public static Color GetIdealForeColor(Color backgroundColor, Color foreColor)
        {
            var bgDelta = Convert.ToInt32((backgroundColor.R * 0.299) + (backgroundColor.G * 0.587) + (backgroundColor.B * 0.114));

            //return (255 - bgDelta < N_THRESHOLD) ? ChangeColorBrightness(foreColor, -1) : ChangeColorBrightness(foreColor, 1);
            return (255 - bgDelta < N_THRESHOLD) ? foreColor : Color.White; //ControlPaint.Dark(foreColor, 50) : ControlPaint.Light(foreColor, 25)
        }

        /// <summary>
        /// Gets the color of the ideal background.
        /// </summary>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <returns></returns>
        public static Color GetIdealBackgroundColor(Color foreColor, Color backgroundColor)
        {
            var foreDelta = Convert.ToInt32((foreColor.R * 0.299) + (foreColor.G * 0.587) + (foreColor.B * 0.114));

            //return (255 - foreDelta < N_THRESHOLD) ? ChangeColorBrightness(bg, -1) : ChangeColorBrightness(bg, 1);
            return (255 - foreDelta < N_THRESHOLD) ? ControlPaint.Light(backgroundColor, 50) : ControlPaint.Dark(backgroundColor, 75);
        }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1.
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color" /> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }
    }
}
