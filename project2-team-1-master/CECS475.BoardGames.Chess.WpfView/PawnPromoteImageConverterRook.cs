using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Cecs475.BoardGames.Chess.Model;

namespace CECS475.BoardGames.Chess.WpfView
{
    class PawnPromoteImageConverterRook : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int player = (int)value;
            if (player == 1)
            {
                return new BitmapImage(new Uri("Chess_rlt60.png", UriKind.Relative));
            }

            if (player == 2)
            {
                return new BitmapImage(new Uri("Chess_rdt60.png", UriKind.Relative));
            }

            return new BitmapImage(new Uri("Chess_rdt60.png", UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
