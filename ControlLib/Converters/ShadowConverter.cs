using ControlLib.Assists;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ControlLib.Converters
{
    public class ShadowConverter : IMultiValueConverter
    {
        public static readonly ShadowConverter Instance = new ShadowConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values[0] is ShadowDepth&&values[1] is ShadowEdges)
            {
                return ShadowInfo.GetDropShadow((ShadowDepth)values[0], (ShadowEdges)values[1]);
            }
            return Binding.DoNothing;
           

           
        }

        public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }

    internal static class ShadowInfo
    {
        public static DropShadowEffect GetDropShadow(ShadowDepth depth,ShadowEdges edges)
        {
            var shadowDepth = ((int)depth) * 2;
            var blurRadius = ((int)depth) * 3 + 5;
            var opacity = 0.42;
            double direction = default(double);
            switch (edges)
            {
                case ShadowEdges.All:
                    shadowDepth = 0;
                    break;
                case ShadowEdges.Left:
                    direction = 180;
                    break;
                case ShadowEdges.Top:
                    direction = 90;
                    break;
                case ShadowEdges.Right:
                    direction = 0;
                    break;
                case ShadowEdges.Bottom:
                    direction = 270;
                    break;
                case ShadowEdges.None:
                    return null;
                default:
                    shadowDepth = 0;
                    break;
            }
            return new DropShadowEffect()
            {
                BlurRadius = blurRadius,
                Color = Colors.Black,
                Direction = direction,
                Opacity = opacity,
                RenderingBias = RenderingBias.Performance,
                ShadowDepth = shadowDepth
            };
        }
    }
}
