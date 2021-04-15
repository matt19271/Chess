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
    public class ChessSquarePlayerConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				ChessPiece p = (ChessPiece) value;

				//ChessPiece p = c.Piece;

				if (p.Player == 1)
				{
					if (p.PieceType.Equals(ChessPieceType.Rook))
					{
						return new BitmapImage(new Uri("Chess_rlt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Pawn))
					{
						return new BitmapImage(new Uri("Chess_plt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Bishop))
					{
						return new BitmapImage(new Uri("Chess_blt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Knight))
					{
						return new BitmapImage(new Uri("Chess_nlt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.King))
					{
						return new BitmapImage(new Uri("Chess_klt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Queen))
					{
						return new BitmapImage(new Uri("Chess_qlt60.png", UriKind.Relative));
					}

				}
				
				// Else if black
				else if (p.Player == 2) 
				{
					if (p.PieceType.Equals(ChessPieceType.Rook))
					{
						return new BitmapImage(new Uri("Chess_rdt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Pawn))
					{
						return new BitmapImage(new Uri("Chess_pdt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Bishop))
					{
						return new BitmapImage(new Uri("Chess_bdt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Knight))
					{
						return new BitmapImage(new Uri("Chess_ndt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.King))
					{
						return new BitmapImage(new Uri("Chess_kdt60.png", UriKind.Relative));
					}

					else if (p.PieceType.Equals(ChessPieceType.Queen))
					{
						return new BitmapImage(new Uri("Chess_qdt60.png", UriKind.Relative));
					}
				}

			
					return null;
				


				//string src = c.ToString().ToLower().Replace(' ', '_');
				//return new BitmapImage(new Uri("/Resources/" + src + ".png", UriKind.Relative));

			}
			catch (Exception e)
			{
				return null;
			}
		}


			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
