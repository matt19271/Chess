using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CECS475.BoardGames.Chess.WpfView
{
    public class ChessSquareBackgroundConverter : IMultiValueConverter
    {
		private static SolidColorBrush HIGHLIGHT_BRUSH = Brushes.Red;
		private static SolidColorBrush WHITE_BRUSH = Brushes.Teal;
		private static SolidColorBrush BLACK_BRUSH = Brushes.LightPink;
		private static SolidColorBrush POSSMOVE_BRUSH = Brushes.LightGreen;
		private static SolidColorBrush DEFAULT_BRUSH = Brushes.LightBlue;
		private static SolidColorBrush CHECK_BRUSH = Brushes.Yellow;


		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			// This converter will receive two properties: the Position of the square, and whether it
			// is being hovered.

			BoardPosition pos = (BoardPosition)values[0];
			bool isHighlighted = (bool)values[1];
			bool isSelected = (bool)values[2];
			bool isGreenHighlighted = (bool)values[3];
			bool isCheck = (bool)values[4];
			//bool SelectedState = (bool)values[3];
			//if(SelectedState == true)
			//{
			//	return POSSMOVE_BRUSH;
			//}
			// Hovered squares have a specific color.
			if (isCheck)
			{
				return CHECK_BRUSH;
			}

			if (isGreenHighlighted)
			{
				return POSSMOVE_BRUSH;
			}

			if (isHighlighted || isSelected)
			{
				return HIGHLIGHT_BRUSH;
			}

			// White Squares
			if ((pos.Row == 0 || pos.Row == 2 || pos.Row == 4 || pos.Row == 6) && (pos.Col == 0 || pos.Col == 2 || pos.Col == 4 || pos.Col == 6) || (pos.Row == 1 || pos.Row == 3 || pos.Row == 5 || pos.Row == 7) && (pos.Col == 1 || pos.Col == 3 || pos.Col == 5 || pos.Col == 7))
			{
				return WHITE_BRUSH;
			}
			
			//black squares
			if ((pos.Row == 0 || pos.Row == 2 || pos.Row == 4 || pos.Row == 6) && (pos.Col == 1 || pos.Col == 3 || pos.Col == 5 || pos.Col == 7) || (pos.Row == 1 || pos.Row == 3 || pos.Row == 5 || pos.Row == 7) && (pos.Col == 0 || pos.Col == 2 || pos.Col == 4 || pos.Col == 6))
			{
				return BLACK_BRUSH;
			}
			//// Squares along the edge are good, and drawn light green.
			//if (pos.Row == 0 || pos.Row == 7 || pos.Col == 0 || pos.Col == 7)
			//{
			//	return SIDE_BRUSH;
			//}
			// Inner squares are drawn light blue.
			return DEFAULT_BRUSH;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
