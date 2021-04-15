using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View
{
	/// <summary>
	/// A chess game view for string-based console input and output.
	/// </summary>
	public class ChessConsoleView : IConsoleView
	{
		private static char[] LABELS = { '.', 'P', 'R', 'N', 'B', 'Q', 'K' };

		// Public methods.
		public string BoardToString(ChessBoard board)
		{
			StringBuilder str = new StringBuilder();

			for (int i = 0; i < ChessBoard.BoardSize; i++)
			{
				str.Append(8 - i);
				str.Append(" ");
				for (int j = 0; j < ChessBoard.BoardSize; j++)
				{
					var space = board.GetPieceAtPosition(new BoardPosition(i, j));
					if (space.PieceType == ChessPieceType.Empty)
						str.Append(". ");
					else if (space.Player == 1)
						str.Append($"{LABELS[(int)space.PieceType]} ");
					else
						str.Append($"{char.ToLower(LABELS[(int)space.PieceType])} ");
				}
				str.AppendLine();
			}
			str.AppendLine("  a b c d e f g h");
			return str.ToString();
		}

		/// <summary>
		/// Converts the given ChessMove to a string representation in the form
		/// "(start, end)", where start and end are board positions in algebraic
		/// notation (e.g., "a5").
		/// 
		/// If this move is a pawn promotion move, the selected promotion piece 
		/// must also be in parentheses after the end position, as in 
		/// "(a7, a8, Queen)".
		/// </summary>
		public string MoveToString(ChessMove move)
		{

			BoardPosition startPos = move.StartPosition;
			BoardPosition endPos = move.EndPosition;

			string startRank = RowToString(startPos.Row);
			string endRank = RowToString(endPos.Row);

			string startFile = ColumnToString(startPos.Col);
			string endFile = ColumnToString(endPos.Col);

			string result;

			if (move.MoveType == ChessMoveType.PawnPromote)
			{
				result = $"({startFile}{startRank}, {endFile}{endRank}, {move.MoveType})";
			}

			else
			{
				result = $"({startFile}{startRank}, {endFile}{endRank})";

			}

			return result;

		}

		private string ColumnToString(int row)
		{
			string rowString = "";

			switch (row)
			{
				case 0:
					rowString = "a";
					break;
				case 1:
					rowString = "b";
					break;
				case 2:
					rowString = "c";
					break;
				case 3:
					rowString = "d";
					break;
				case 4:
					rowString = "e";
					break;
				case 5:
					rowString = "f";
					break;
				case 6:
					rowString = "g";
					break;
				case 7:
					rowString = "h";
					break;
			}

			return rowString;
		}

		private string RowToString(int col)
		{
			string colString = "";

			switch (col)
			{
				case 7:
					colString = "1";
					break;
				case 6:
					colString = "2";
					break;
				case 5:
					colString = "3";
					break;
				case 4:
					colString = "4";
					break;
				case 3:
					colString = "5";
					break;
				case 2:
					colString = "6";
					break;
				case 1:
					colString = "7";
					break;
				case 0:
					colString = "8";
					break;
			}

			return colString;
		}

		private int CharToCol(char row)
		{
			int rowString = 0;

			switch (row)
			{
				case 'a':
					rowString = 0;
					break;
				case 'b':
					rowString = 1;
					break;
				case 'c':
					rowString = 2;
					break;
				case 'd':
					rowString = 3;
					break;
				case 'e':
					rowString = 4;
					break;
				case 'f':
					rowString = 5;
					break;
				case 'g':
					rowString = 6;
					break;
				case 'h':
					rowString = 7;
					break;
			}

			return rowString;
		}

		private int CharToRow(char col)
		{
			int colString = 0;

			switch (col)
			{
				case '1':
					colString = 7;
					break;
				case '2':
					colString = 6;
					break;
				case '3':
					colString = 5;
					break;
				case '4':
					colString = 4;
					break;
				case '5':
					colString = 3;
					break;
				case '6':
					colString = 2;
					break;
				case '7':
					colString = 1;
					break;
				case '8':
					colString = 0;
					break;
			}

			return colString;
		}



		public string PlayerToString(int player)
		{
			return player == 1 ? "White" : "Black";
		}

		/// <summary>
		/// Converts a string representation of a move into a ChessMove object.
		/// Must work with any string representation created by MoveToString.
		/// </summary>
		public ChessMove ParseMove(string moveText)
		{

			char[] delimiterChars = { '(', ',', ')', ' ', '\t' };
			string[] words = moveText.Split(delimiterChars);
			List<string> moves = new List<string>();

			int startRow;
			int startCol;

			int endRow;
			int endCol;

			foreach (var word in words)
			{
				if (word.Equals(""))
				{
					continue;
				}

				else
				{
					moves.Add(word);
				}

			}

			startCol = CharToCol(moves[0][0]);
			startRow = CharToRow(moves[0][1]);

			endCol = CharToCol(moves[1][0]);
			endRow = CharToRow(moves[1][1]);

			BoardPosition startPos = new BoardPosition(startRow, startCol);
			BoardPosition endPos = new BoardPosition(endRow, endCol);

			if (moves.Count > 2)
			{

				ChessPieceType t = ChessPieceType.Queen;

				if (moves[2].ToLower().Equals("queen"))
				{
					t = ChessPieceType.Queen;
				}

				else if (moves[2].ToLower().Equals("bishop"))
				{
					t = ChessPieceType.Bishop;
				}

				else if (moves[2].ToLower().Equals("knight"))
				{
					t = ChessPieceType.Knight;
				}

				else if (moves[2].ToLower().Equals("rook"))
				{
					t = ChessPieceType.Rook;
				}

				return new ChessMove(startPos, endPos, t, ChessMoveType.PawnPromote);
			}
			else
			{
				return new ChessMove(startPos, endPos);
			}
		}

		public static BoardPosition ParsePosition(string pos)
		{
			return new BoardPosition(8 - (pos[1] - '0'), pos[0] - 'a');
		}

		public static string PositionToString(BoardPosition pos)
		{
			return $"{(char)(pos.Col + 'a')}{8 - pos.Row}";
		}

		#region Explicit interface implementations
		// Explicit method implementations. Do not modify these.
		string IConsoleView.BoardToString(IGameBoard board)
		{
			return BoardToString(board as ChessBoard);
		}

		string IConsoleView.MoveToString(IGameMove move)
		{
			return MoveToString(move as ChessMove);
		}

		IGameMove IConsoleView.ParseMove(string moveText)
		{
			return ParseMove(moveText);
		}
		#endregion
	}
}
