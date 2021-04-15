using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard {
		#region Member fields.
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();

		public const int BoardSize = 8;

		// TODO: create a field for the board position array. You can hand-initialize
		// the starting entries of the array, or set them in the constructor.

		// Change this to use SetPieceAtPosition in ChessBoard constructor instead 
		public byte[] board = 
					   {0b10101011, 0b11001101, 0b11101100, 0b10111010,
						0b10011001, 0b10011001, 0b10011001, 0b10011001,
						0b00000000, 0b00000000, 0b00000000, 0b00000000,
						0b00000000, 0b00000000, 0b00000000, 0b00000000,
						0b00000000, 0b00000000, 0b00000000, 0b00000000,
						0b00000000, 0b00000000, 0b00000000, 0b00000000,
						0b00010001, 0b00010001, 0b00010001, 0b00010001,
						0b00100011, 0b01000101, 0b01100100, 0b00110010};

		// TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.

		// TODO: add a field for tracking the current player and the board advantage.	

		private int currentPlayer = 1;
		public bool isFinished;
		private int mDrawCounter = 0;

		private Stack<ChessPiece> capturedPieces = new Stack<ChessPiece>();
		private Stack<int> previousDrawCount = new Stack<int>(new int[] { 0 });

		private int player1total;
		private int player2total;

		private List<ChessMove> possibleMoves = new List<ChessMove>();

		private List<ChessMove> enPassantMovesWhite = new List<ChessMove>();
		private List<ChessMove> enPassantMovesBlack = new List<ChessMove>();

		private Stack<int> p1Stack = new Stack<int>(new int[] { 0 });
		private Stack<int> p2Stack = new Stack<int>(new int[] { 0 });

		#endregion

		#region Properties.
		// TODO: implement these properties.
		// You can choose to use auto properties, computed properties, or normal properties 
		// using a private field to back the property.

		// You can add set bodies if you think that is appropriate, as long as you justify
		// the access level (public, private).

		public bool IsFinished
		{
			get
			{
				if (IsStalemate == true)
				{
					return true;
				}

				else if(IsCheckmate == true){
					return true;
				}

				else if(IsDraw == true)
				{
					return true;
				}

				else
				{
					return false;
				}
			}
		}


		public int CurrentPlayer { get { return currentPlayer; } }

		public GameAdvantage CurrentAdvantage
		{
			get
			{

					int adv = player1total - player2total;
					if (adv > 0)
					{
						return new GameAdvantage(1, Math.Abs(adv));
					}
					else if (adv < 0)
					{
						return new GameAdvantage(2, Math.Abs(adv));
					}
					else
					{
						return new GameAdvantage(0, 0);
					}

				
			}
		 }


		public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

		// TODO: implement IsCheck, IsCheckmate, IsStalemate
		public bool IsCheck {
			get
			{
				if (GetPossibleMoves()?.Any() != true)
				{
					return false;
				}

				if (currentPlayer == 1)
				{
					foreach (BoardPosition pos in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
					{
						if (GetPieceAtPosition(pos).Player == 1 && GetPieceAtPosition(pos).PieceType == ChessPieceType.King)
						{
							if (PositionIsAttacked(pos, 2))
							{
								return true;
							}
						}
					}
					return false;
				}

				else if (currentPlayer == 2)
				{
					foreach (BoardPosition pos in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
					{
						if (GetPieceAtPosition(pos).Player == 2 && GetPieceAtPosition(pos).PieceType == ChessPieceType.King)
						{
							if (PositionIsAttacked(pos, 1))
							{
								return true;
							}
						}
					}
					return false;
				}
				else 
					return false;
			}
		}

		public bool IsCheckmate {
			get {
				//no moves left
				if (GetPossibleMoves()?.Any() != true && checkMe())
				{
					//IsCheck = false;
					isFinished = true;
					return true;
				}

				else
				{
					return false;
				}
			
			}
		}

		public bool IsStalemate {
			get
			{
				if (GetPossibleMoves()?.Any() != true && checkMe() == false)
				{
					isFinished = true;
					return true;
				}

				else
				{
					return false;
				}
			}
		}

		public bool IsDraw {
			get
			{
				if (mDrawCounter == 100)
				{
					isFinished = true;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		
		/// <summary>
		/// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
		/// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
		/// </summary>
		public int DrawCounter {
			get { return mDrawCounter; }
		}
		#endregion


		#region Public methods.
		public IEnumerable<ChessMove> GetPossibleMoves()
		{

			possibleMoves.Clear();

			possibleMoves.AddRange(enPassantMovesBlack);
			possibleMoves.AddRange(enPassantMovesWhite);

			//any move is possible if its in bounds, its within a pieces move logic, and it doesnt put the king in check
			//List<ChessMove> possibleMoves = new List<ChessMove>();
			BoardPosition kingLocation = new BoardPosition(0,0);
			foreach (BoardPosition pos in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
			{
				HashSet<BoardPosition> positions = new HashSet<BoardPosition>();
				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Bishop && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					positions.UnionWith(GetBishopLogic(positions, pos));
				}
				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Queen && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					positions.UnionWith(GetQueenLogic(positions, pos));
				}
				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.King && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					kingLocation = pos;
					HashSet<BoardPosition> tempKingSpots = new HashSet<BoardPosition>();
					tempKingSpots.UnionWith(GetKingLogic(positions, pos));
					foreach (BoardPosition p in tempKingSpots)
					{
						//check if king moves into check
						int otherPlayer;
						if (currentPlayer == 1)
						{
							otherPlayer = 2;
						}
						else
						{
							otherPlayer = 1;
						}

						if (!PositionIsAttacked(p, otherPlayer))
						{
							positions.Add(p);
						}
					}

				}
				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Rook && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					positions.UnionWith(GetRookLogic(positions, pos));
				}
				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Pawn && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					if (currentPlayer == 1)
					{
						BoardPosition temp = pos;

						// If move is a pawn promotion
						if (temp.Translate(-1, 0).Row == 0 && (GetPieceAtPosition(temp.Translate(-1, 0)).PieceType == ChessPieceType.Empty))
						{
							possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 0), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 0), ChessPieceType.Rook, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 0), ChessPieceType.Knight, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 0), ChessPieceType.Queen, ChessMoveType.PawnPromote));
						}

						//up and right
						if (ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true && (GetPieceAtPosition(temp.Translate(-1, 1)).Player == 2))
						{

							if (temp.Translate(-1, 1).Row == 0)
							{
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 1), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 1), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 1), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, 1), ChessPieceType.Queen, ChessMoveType.PawnPromote));
							}
							else
							{
								temp = temp.Translate(-1, 1);
								positions.Add(temp);
							}

						}
						temp = pos;
						//up and left
						if (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true && (GetPieceAtPosition(temp.Translate(-1, -1)).Player == 2))
						{
							if (temp.Translate(-1, -1).Row == 0)
							{
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, -1), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, -1), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, -1), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(-1, -1), ChessPieceType.Queen, ChessMoveType.PawnPromote));
							}
							else
							{
								temp = temp.Translate(-1, -1);
								positions.Add(temp);
							}
						}
						temp = pos;
						//up
						if (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true && (GetPieceAtPosition(temp.Translate(-1, 0)).PieceType == ChessPieceType.Empty) && temp.Translate(-1, 0).Row != 0)
						{
							temp = temp.Translate(-1, 0);
							positions.Add(temp);
							// Moving white pawn two spaces forward
							if (pos.Row == 6 && (GetPieceAtPosition(temp.Translate(-1, 0)).PieceType == ChessPieceType.Empty))
							{
								temp = pos;
								temp = temp.Translate(-2, 0);
								positions.Add(temp);

							} // End of two-space forward checks

						}

					}
					if (currentPlayer == 2)
					{
						BoardPosition temp = pos;

						// If move is a pawn promotion
						if (temp.Translate(1, 0).Row == 7 && (GetPieceAtPosition(temp.Translate(1, 0)).PieceType == ChessPieceType.Empty))
						{
							possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 0), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 0), ChessPieceType.Rook, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 0), ChessPieceType.Knight, ChessMoveType.PawnPromote));
							possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 0), ChessPieceType.Queen, ChessMoveType.PawnPromote));
						}

						// Else if not a pawn promotion
						// Down and left
						if (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true && (GetPieceAtPosition(temp.Translate(1, -1)).Player == 1))
						{

							if (temp.Translate(1, -1).Row == 7)
							{
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, -1), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, -1), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, -1), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, -1), ChessPieceType.Queen, ChessMoveType.PawnPromote));
							}
							else
							{
								temp = temp.Translate(1, -1);
								positions.Add(temp);
							}
						}
						temp = pos;
						//down and right
						if (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true && (GetPieceAtPosition(temp.Translate(1, 1)).Player == 1))
						{
							if (temp.Translate(1, 1).Row == 7)
							{
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 1), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 1), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 1), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(temp, temp.Translate(1, 1), ChessPieceType.Queen, ChessMoveType.PawnPromote));
							}
							else
							{
								temp = temp.Translate(1, 1);
								positions.Add(temp);
							}

						}
						temp = pos;
						//down
						if (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true && (GetPieceAtPosition(temp.Translate(1, 0)).PieceType == ChessPieceType.Empty) && temp.Translate(1, 0).Row != 7)
						{
							temp = temp.Translate(1, 0);
							positions.Add(temp);
							// Moving black pawn two spaces forward (aka down)
							if (pos.Row == 1 && (GetPieceAtPosition(temp.Translate(1, 0)).PieceType == ChessPieceType.Empty))
							{
								temp = pos;
								temp = temp.Translate(2, 0);
								positions.Add(temp);
							}
						}

					}

					if (mMoveHistory.Count > 0)
					{
						ChessMove m = mMoveHistory.ElementAt(mMoveHistory.Count - 1);

						if (((m.StartPosition.Row == 1 && m.EndPosition.Row == 3) || (m.StartPosition.Row == 6 && m.EndPosition.Row == 4)) && (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Pawn))
						{

							if (currentPlayer == 1)
							{
								// If there is a pawn that belongs to the current player to the right of this pawn
								if ((GetPieceAtPosition(m.EndPosition.Translate(0, 1)).PieceType == ChessPieceType.Pawn) && (GetPieceAtPosition(m.EndPosition.Translate(0, 1)).Player == CurrentPlayer) && m.EndPosition.Translate(0, 1) == pos && PositionInBounds(m.EndPosition.Translate(0, 1)))
								{
									possibleMoves.Add(new ChessMove(m.EndPosition.Translate(0, 1), m.EndPosition.Translate(-1, 0), ChessMoveType.EnPassant));
									possibleMoves.ElementAt(possibleMoves.Count - 1).Player = 1;
								}

								if ((GetPieceAtPosition(m.EndPosition.Translate(0, -1)).PieceType == ChessPieceType.Pawn) && (GetPieceAtPosition(m.EndPosition.Translate(0, -1)).Player == CurrentPlayer) && m.EndPosition.Translate(0, -1) == pos && PositionInBounds(m.EndPosition.Translate(0, -1)))
								{
									// If there is a pawn that belongs to the current player to the left of this pawn
									possibleMoves.Add(new ChessMove(m.EndPosition.Translate(0, -1), m.EndPosition.Translate(-1, 0), ChessMoveType.EnPassant));
									possibleMoves.ElementAt(possibleMoves.Count - 1).Player = 1;
								}
								//possibleMoves.AddRange(enPassantMovesBlack);
							}
							else
							{
								if ((GetPieceAtPosition(m.EndPosition.Translate(0, 1)).PieceType == ChessPieceType.Pawn) || (GetPieceAtPosition(m.EndPosition.Translate(0, -1)).PieceType == ChessPieceType.Pawn))
								{
									// If there is a pawn that belongs to the current player to the right of this pawn
									if ((GetPieceAtPosition(m.EndPosition.Translate(0, 1)).PieceType == ChessPieceType.Pawn) && (GetPieceAtPosition(m.EndPosition.Translate(0, 1)).Player == CurrentPlayer) && m.EndPosition.Translate(0, 1) == pos && PositionInBounds(m.EndPosition.Translate(0, 1)))
									{
										possibleMoves.Add(new ChessMove(m.EndPosition.Translate(0, 1), m.EndPosition.Translate(1, 0), ChessMoveType.EnPassant));
										possibleMoves.ElementAt(possibleMoves.Count - 1).Player = 2;
									}

									// If there is a pawn that belongs to the current player to the left of this pawn
									if ((GetPieceAtPosition(m.EndPosition.Translate(0, -1)).PieceType == ChessPieceType.Pawn) && (GetPieceAtPosition(m.EndPosition.Translate(0, -1)).Player == CurrentPlayer) && m.EndPosition.Translate(0, -1) == pos && PositionInBounds(m.EndPosition.Translate(0, -1)))
									{
										possibleMoves.Add(new ChessMove(m.EndPosition.Translate(0, -1), m.EndPosition.Translate(1, 0), ChessMoveType.EnPassant));
										possibleMoves.ElementAt(possibleMoves.Count - 1).Player = 2;
									}
								}
								//possibleMoves.AddRange(enPassantMovesWhite);
							}
						}

					}
				}

				if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Knight && (GetPieceAtPosition(pos).Player == currentPlayer))
				{
					positions.UnionWith(GetKnightLogic(positions, pos));
				}
				//check
				// this includes if it puts king in check etc.
				//add em all
				/*
				 * Makes no sense so im trying this :(
				 */
				HashSet<BoardPosition> tempPositions = new HashSet<BoardPosition>();
				tempPositions.UnionWith(positions);
				foreach (BoardPosition p in tempPositions)
				{
					// DEBUG: check if players are 1 and 2 or 0 and 1 for empty piece [0000] 
					if (GetPieceAtPosition(p).Player != currentPlayer)
					{
						possibleMoves.Add(new ChessMove(pos, p, ChessMoveType.Normal));
						possibleMoves.ElementAt(possibleMoves.Count - 1).Player = CurrentPlayer;
					}
				}
			}

			//check for castling
			possibleMoves.AddRange(possibleCastlingMoves());
			////check if any put the king in check

			List<ChessMove> tempList = new List<ChessMove>();
			//foreach (ChessMove m in possibleMoves)
			//int oPlayer;
			//if (currentPlayer == 1)
			//{
			//	oPlayer = 2;
			//}
			//else
			//{
			//	oPlayer = 1;
			//}
			for (int i = possibleMoves.Count - 1; i >= 0; i--)
			{
				hypotheticalApplyMove(possibleMoves.ElementAt(i));
				if (checkMe() == false)
				{
					tempList.Add(possibleMoves.ElementAt(i));
				}
				hypotheticalUndoLastMove();
			}

			//List<ChessMove> tempList = possibleMoves;
			//foreach (ChessMove move in tempList)
			//{
			//	ApplyMove(move);
			//	if (playerBecameInCheck(kingLocation))
			//	{
			//		possibleMoves.Remove(move);
			//	}
			//	UndoLastMove();
			//}
			//if (currentPlayer == 1)
			//{
			//	//possibleMoves.AddRange(enPassantMovesWhite);
			//	possibleMoves.AddRange(enPassantMovesWhite);
			//}

			//else
			//{
			//	possibleMoves.AddRange(enPassantMovesBlack);
			//}

			//enPassantMovesBlack.Clear();
			//enPassantMovesWhite.Clear();
			//possibleMoves = possibleMoves.Distinct().ToList();
			return tempList;//possibleMoves;
		}

		public bool checkMe()
		{
			if (currentPlayer == 1)
			{
				foreach (BoardPosition pos in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
				{
					if (GetPieceAtPosition(pos).Player == 1 && GetPieceAtPosition(pos).PieceType == ChessPieceType.King)
					{
						if (PositionIsAttacked(pos, 2))
						{
							return true;
						}
					}
				}
				return false;
			}

			else if (currentPlayer == 2)
			{
				foreach (BoardPosition pos in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
				{
					if (GetPieceAtPosition(pos).Player == 2 && GetPieceAtPosition(pos).PieceType == ChessPieceType.King)
					{
						if (PositionIsAttacked(pos, 1))
						{
							return true;
						}
					}
				}
				return false;
			}
			else
				return false;
		}

		public void ApplyMove(ChessMove m)
		{
			//throw new InvalidOperationException($"this is m promo type{m.PromoType}");
			//enPassantMovesBlack.Clear();
			//enPassantMovesWhite.Clear();
			if ( ((m.StartPosition.Row == 1 && m.EndPosition.Row == 3) || (m.StartPosition.Row == 6 && m.EndPosition.Row == 4)) && (GetPieceAtPosition(m.StartPosition).PieceType == ChessPieceType.Pawn))		
			{
				capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				// Set the moved piece's position to the square it's being moved to
				SetPieceAtPosition(m.EndPosition, new ChessPiece(GetPieceAtPosition(m.StartPosition).PieceType, currentPlayer));
				// Set the moved piece's start position to empty 
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				mDrawCounter = 0;

			} // End of if double jump

			else {


				// If the move is en passant:
				if (m.MoveType == ChessMoveType.EnPassant)
				{

					enPassantMovesBlack.Clear();
					enPassantMovesWhite.Clear();
					// If white pawn is capturing a black pawn with en passant
					if (currentPlayer == 1)
					{
						// 1. Set the current player's pawn to the new position
						SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Pawn, 1));//GetPieceAtPosition(m.StartPosition));
																								  // 2. Add the captured black pawn to captured pieces list
																								  //capturedPieces.Push(GetPieceAtPosition(m.EndPosition.Translate(1, 0)));
						capturedPieces.Push(new ChessPiece(ChessPieceType.Pawn, 2));//GetPieceAtPosition(m.EndPosition.Translate(1, 0)).PieceType, 2));
						SetPieceAtPosition(m.EndPosition.Translate(1, 0), new ChessPiece(ChessPieceType.Empty, 0));
						// 3. Set the current player's previous pawn position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
						player2total--;
					}
					// Else if black pawn is capturing a white pawn with en passant
					else
					{
						// 1. Set the current player's pawn to the new position
						SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Pawn, 2));
						// 2. Add the captured black pawn to captured pieces list
						//capturedPieces.Push(GetPieceAtPosition(m.EndPosition.Translate(-1, 0)));
						capturedPieces.Push(new ChessPiece(ChessPieceType.Pawn, 1)); //capturedPieces.Push(new ChessPiece(GetPieceAtPosition(m.EndPosition.Translate(1, 0)).PieceType, 1));
						SetPieceAtPosition(m.EndPosition.Translate(-1, 0), new ChessPiece(ChessPieceType.Empty, 0));
						// 3. Set the current player's previous pawn position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
						player1total--;
					}
					mDrawCounter = 0;
				} // End of if EnPassant


				if (m.MoveType == ChessMoveType.PawnPromote)
				{
					
					// If a piece is not being captured during the pawn promote
					if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Empty)
					{
						capturedPieces.Push(new ChessPiece(0, 0));

						if (currentPlayer == 1)
						{
							//player1total += GetPieceAtPosition(m.EndPosition).Value;
							player1total += new ChessPiece(m.PromoType, 1).Value;
							player1total--;
						}

						else
						{
							player2total += new ChessPiece(m.PromoType, 2).Value;
							player2total--;
						}
					}

					// If a piece is being captured during the pawn promote
					else
					{
						// Push the captured piece to the captured pieces stack
						capturedPieces.Push(GetPieceAtPosition(m.EndPosition));

						if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Pawn)
						{
							if (currentPlayer == 2)
							{
								player1total--;
							}
							else
							{
								player2total--;
							}
						}

						else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Knight || GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Bishop)
						{
							if (currentPlayer == 2)
							{
								player1total -= 3;
							}
							else
							{
								player2total -= 3;
							}
						}

						else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Rook)
						{
							if (currentPlayer == 2)
							{
								player1total -= 5;
							}
							else
							{
								player2total -= 5;
							}
						}

						else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Queen)
						{
							if (currentPlayer == 2)
							{
								player1total -= 9;
							}
							else
							{
								player2total -= 9;
							}
						}


						mDrawCounter = 0;

						if (currentPlayer == 1)
						{
							//player1total += GetPieceAtPosition(m.EndPosition).Value;
							player1total += new ChessPiece(m.PromoType, 1).Value;
							player1total--;
						}

						else
						{
							player2total += new ChessPiece(m.PromoType, 2).Value;
							player2total--;
						}



					}
					
					// Set the moved piece to the end position and make it the type it is being promoted to
					SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PromoType, GetPieceAtPosition(m.StartPosition).Player));
					// Set the pawn's previous position to empty
					SetPieceAtPosition(m.StartPosition, new ChessPiece(0, 0));

					//if (currentPlayer == 1)
					//	{
					//		player1total += GetPieceAtPosition(m.EndPosition).Value;
					//		player1total--;
					//	}

					//	else
					//	{
					//		player2total += GetPieceAtPosition(m.EndPosition).Value;
					//		player2total--;
					//	}

				}


				if (m.MoveType == ChessMoveType.CastleKingSide)
				{
					capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));

					//set the rook
					if (currentPlayer == 1)
					{
						SetPieceAtPosition(new BoardPosition(7, 5), GetPieceAtPosition(new BoardPosition(7, 7)));
						SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Empty, 0));
					}
					if (currentPlayer == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 5), GetPieceAtPosition(new BoardPosition(0, 7)));
						SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Empty, 0));
					}
					//set the king
					SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
					mDrawCounter++;
				}
				//If the move type is castling queen side
				if (m.MoveType == ChessMoveType.CastleQueenSide)
				{

					capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));

					//set the rook
					if (currentPlayer == 1)
					{
						SetPieceAtPosition(new BoardPosition(7, 3), GetPieceAtPosition(new BoardPosition(7, 0)));
						SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Empty, 0));
					}
					if (currentPlayer == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 3), GetPieceAtPosition(new BoardPosition(0, 0)));
						SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Empty, 0));
					}
					//set the king
					SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
					mDrawCounter++;
				}

				else
				{
					enPassantMovesBlack.Clear();
					enPassantMovesWhite.Clear();
					if (!PositionIsEmpty(m.StartPosition))
					{
						// If the position contains an enemy piece
						if (!PositionIsEmpty(m.EndPosition)) //puppy
						{
							// Add newly-captured enemy piece to capturedPieces stack
							capturedPieces.Push(GetPieceAtPosition(m.EndPosition));

							if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Pawn)
							{
								if (currentPlayer == 2)
								{
									player1total--;
								}
								else
								{
									player2total--;
								}
							}

							else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Knight || GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Bishop)
							{
								if (currentPlayer == 2)
								{
									player1total -= 3;
								}
								else
								{
									player2total -= 3;
								}
							}

							else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Rook)
							{
								if (currentPlayer == 2)
								{
									player1total -= 5;
								}
								else
								{
									player2total -= 5;
								}
							}

							else if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Queen)
							{
								if (currentPlayer == 2)
								{
									player1total -= 9;
								}
								else
								{
									player2total -= 9;
								}
							}


							mDrawCounter = 0;
						}
						// If the position is empty
						else
						{
							// Add an empty piece to the capturedPieces stack
							capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));
							mDrawCounter++;
						}

						// Deal with mDrawCounter
						//if (GetPieceAtPosition(m.EndPosition).PieceType != ChessPieceType.Pawn)
						//{
						//	mDrawCounter++;
						//}
						//else
						//{
						//	mDrawCounter = 0;
						//}

						SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));

						// Set the moved piece's position to the square it's being moved to
						SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
						// Set the moved piece's start position to empty 
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));

					}



				}
			}

			p1Stack.Push(player1total);
			p2Stack.Push(player2total);

			//change players
			if (currentPlayer == 1)
			{
				currentPlayer = 2;
			}
			else
			{
				currentPlayer = 1;
			}

			previousDrawCount.Push(mDrawCounter);
			m.Player = currentPlayer;
			mMoveHistory.Add(m);
			//possibleMoves.Clear();
			//enPassantMovesBlack.Clear();
			//enPassantMovesWhite.Clear();

		}

		public void UndoLastMove()
		{
			if(mMoveHistory.Count == 0)
			{
				throw new InvalidOperationException("There are no moves to undo!");
			}

			ChessMove lastMove = mMoveHistory[mMoveHistory.Count - 1];

			BoardPosition lastMoveStart = lastMove.StartPosition;
			BoardPosition lastMoveEnd = lastMove.EndPosition;
			int player = lastMove.Player;

			ChessPiece pieceMoved = GetPieceAtPosition(lastMoveEnd);

			ChessPiece possibleCapture = capturedPieces.Pop();

			// If the move being undone did not have a capture
			if (possibleCapture.PieceType == ChessPieceType.Empty)
			{
				// If the last move was a pawn promotion, set piece back to pawn
				if (lastMove.MoveType == ChessMoveType.PawnPromote)
				{

					//if (currentPlayer == 1)
					//{
					//	player1total -= GetPieceAtPosition(lastMoveEnd).Value;
					//	player1total++;
					//}

					//else
					//{
					//	player2total -= GetPieceAtPosition(lastMoveEnd).Value;
					//	player2total++;
					//}

					SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, GetPieceAtPosition(lastMoveEnd).Player));
					// Set pawn's previous position to empty
					SetPieceAtPosition(lastMoveEnd, new ChessPiece(0, 0));

				}

				else if (lastMove.MoveType == ChessMoveType.CastleKingSide || lastMove.MoveType == ChessMoveType.CastleQueenSide)
				{
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 1)
					{
						//kingside white
						if (lastMoveEnd.Equals(new BoardPosition(7, 6)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 1));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1));
							//SetPieceAtPosition(new BoardPosition(7, 7), GetPieceAtPosition(new BoardPosition(7, 5)));
							SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 2)
					{
						//kingside black
						if (lastMoveEnd.Equals(new BoardPosition(0, 6)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 2));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2));
							//SetPieceAtPosition(new BoardPosition(0, 7), GetPieceAtPosition(new BoardPosition(0, 5)));
							SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 1)
					{
						//Queenside white
						if (lastMoveEnd.Equals(new BoardPosition(7, 2)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 1));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1));
							//SetPieceAtPosition(new BoardPosition(7, 0), GetPieceAtPosition(new BoardPosition(7, 3)));
							SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 2)
					{
						//queenside black
						if (lastMoveEnd.Equals(new BoardPosition(0, 2)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 2));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2));
							//SetPieceAtPosition(new BoardPosition(0, 0), GetPieceAtPosition(new BoardPosition(0, 3)));
							SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
				}



				else
				{
					SetPieceAtPosition(lastMoveStart, pieceMoved);
					SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
				}


			}

			// If a piece was captured during the last move
			else
			{

				//if (possibleCapture.PieceType == ChessPieceType.Pawn)
				//{
				//	if (possibleCapture.Player == 2)
				//	{
				//		player2total++;
				//	}
				//	else
				//	{
				//		player1total++;
				//	}
				//}
				//else if (possibleCapture.PieceType == ChessPieceType.Knight || possibleCapture.PieceType == ChessPieceType.Bishop)
				//{
				//	if (possibleCapture.Player == 2)
				//	{
				//		player2total += 3;
				//	}
				//	else
				//	{
				//		player1total += 3;
				//	}
				//}

				//else if (possibleCapture.PieceType == ChessPieceType.Rook)
				//{
				//	if (possibleCapture.Player == 2)
				//	{
				//		player2total += 5;
				//	}
				//	else
				//	{
				//		player1total += 5;
				//	}
				//}

				//else if (possibleCapture.PieceType == ChessPieceType.Queen)
				//{
				//	if (possibleCapture.Player == 2)
				//	{
				//		player2total += 9;
				//	}
				//	else
				//	{
				//		player1total += 9;
				//	}
				//}

			

				// If the last move was a pawn promotion, set piece back to pawn
				if (lastMove.MoveType == ChessMoveType.PawnPromote)
				{

					//if (currentPlayer == 1)
					//{
					//	player1total -= new ChessPiece(lastMove.PromoType, 1).Value;
					//	player1total++;
					//}

					//else
					//{
					//	player2total -= new ChessPiece(lastMove.PromoType, 2).Value;
					//	player2total++;
					//}

					SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, GetPieceAtPosition(lastMoveEnd).Player));
					SetPieceAtPosition(lastMoveEnd, possibleCapture);



				}

				else if (lastMove.MoveType == ChessMoveType.EnPassant)
				{

					if (possibleCapture.Player == 2)
					{
						SetPieceAtPosition(lastMoveEnd.Translate(1, 0), possibleCapture);
						SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, 1));
						SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
						
					}

					else
					{
						SetPieceAtPosition(lastMoveEnd.Translate(-1, 0), possibleCapture);
						SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, 2));
						SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
						
					}

				}

				else
				{
					SetPieceAtPosition(lastMoveStart, pieceMoved);
					SetPieceAtPosition(lastMoveEnd, possibleCapture);
				}



			}

			previousDrawCount.Pop();
			mDrawCounter = previousDrawCount.Peek();
			mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
			p1Stack.Pop();
			player1total = p1Stack.Peek();
			p2Stack.Pop();
			player2total = p2Stack.Peek();


			if (currentPlayer == 1)
			{
				currentPlayer = 2;
			}
			
			else
			{
				currentPlayer = 1;
			}
		}

		/// <summary>
		/// Returns whatever chess piece is occupying the given position.
		/// </summary>
		public ChessPiece GetPieceAtPosition(BoardPosition position)
		{

			int index = position.Row * 4;
			index += position.Col / 2;
			int player;

			byte square = board[index];

			if (position.Col % 2 == 0)
			{
				// then leftmost bits
				square = (byte)(square & 0b11110000);
				square = (byte)(square >> 4);
			}
			else
			{ // else rightmost bits
				square = (byte)(square & 0b00001111);
			}

			player = (square & 0b00001000) >> 3;
			
			if (square == 0b0000)
			{
				player = 0;
			}
			else if (player == 0)
			{
				player = 1;
			}
			else
			{
				player = 2;
			}

			square = (byte)(square & 0b00000111);

			ChessPiece piece = new ChessPiece((ChessPieceType)(square), player);

			return piece;
		}

		public bool playerBecameInCheck(BoardPosition pos)
		{
			if (PositionIsAttacked(pos, CurrentPlayer))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns whatever player is occupying the given position.
		/// </summary>
		public int GetPlayerAtPosition(BoardPosition pos) {
			// As a hint, you should call GetPieceAtPosition.
			ChessPiece piece = GetPieceAtPosition(pos);
			int playerAtPos = (int) piece.Player;

			return playerAtPos;
		}

		/// <summary>
		/// Returns true if the given position on the board is empty.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEmpty(BoardPosition pos) {
			ChessPiece piece = GetPieceAtPosition(pos);

			return (piece.PieceType == ChessPieceType.Empty);
		}

		/// <summary>
		/// Returns true if the given position contains a piece that is the enemy of the given player.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEnemy(BoardPosition pos, int player) {
			ChessPiece piece = GetPieceAtPosition(pos);
			int playerAtPos = piece.Player;

			if (playerAtPos == 0)
			{
				return false;
			}
			else
			{
				return playerAtPos != player;
			}
		}

		/// <summary>
		/// Returns true if the given position is in the bounds of the board.
		/// </summary>
		public static bool PositionInBounds(BoardPosition pos) {

			return ((pos.Row < 8 && pos.Row >= 0) && (pos.Col < 8 && pos.Col >= 0));
		}

		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player) {
			IEnumerable <BoardPosition> positions = BoardPosition.GetRectangularPositions(8, 8);
			List<BoardPosition> piecePositions = new List<BoardPosition>();
			foreach (BoardPosition pos in positions)
			{
				ChessPiece p = GetPieceAtPosition(pos);
				if (p.PieceType == piece && p.Player == player)
				{
					piecePositions.Add(pos);
				}
			}

			return piecePositions;
		}

		/// <summary>
		/// Returns true if the given player's pieces are attacking the given position.
		/// </summary>
		public bool PositionIsAttacked(BoardPosition position, int byPlayer) {
			HashSet<BoardPosition> attackedPositions = new HashSet<BoardPosition>();
			attackedPositions.UnionWith(GetAttackedPositions(byPlayer));
			//definitley has something in it cheked using if count == 0 and ==1 in case null
			if (attackedPositions.Contains(position))
			{
				return true;
			}
			else
			{
				return false;
			}
			//foreach(BoardPosition element in attackedPositions)
			//{
			//	//assuming .equals works for position objects may need to change
			//	if (position.Equals(element)){
			//		return true;
			//	}
			//}
			//return false;
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player.
		/// </summary>
		public ISet<BoardPosition> GetAttackedPositions(int byPlayer) {
			HashSet<BoardPosition> positions = new HashSet<BoardPosition>();
			foreach (BoardPosition p in BoardPosition.GetRectangularPositions(BoardSize, BoardSize))
			{
				BoardPosition temp = p;
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.Bishop && (GetPieceAtPosition(p).Player == byPlayer))
				{
					positions.UnionWith(GetBishopLogic(positions, temp));
				}
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.Queen && (GetPieceAtPosition(p).Player == byPlayer))
				{
					//positions.Add(new BoardPosition(5, 4));
					positions.UnionWith(GetQueenLogic(positions, temp));
				}
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.King && (GetPieceAtPosition(p).Player == byPlayer))
				{
					positions.UnionWith(GetKingLogic(positions, temp));
				}
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.Rook && (GetPieceAtPosition(p).Player == byPlayer))
				{
					positions.UnionWith(GetRookLogic(positions, temp));
				}
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.Pawn && (GetPieceAtPosition(p).Player == byPlayer))
				{
					if (byPlayer == 1)
					{
						temp = p;
						//up and right
						if (ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true)
						{
							temp = temp.Translate(-1, 1);
							positions.Add(temp);

						}
						//added this
						temp = p;
						//up and left
						if (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
						{
							temp = temp.Translate(-1, -1);
							positions.Add(temp);

						}
					}
					if (byPlayer == 2)
					{
						temp = p;
						/*
						 * player two moves down the board
						 * must write if statement for if its the starting turn then add move forward two
						 * also if an enemy is to the diagonal then a possible move is there
						 * also if enpassant is possible add that as well
						*/
						//down and left
						if (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
						{
							temp = temp.Translate(1, -1);
							positions.Add(temp);

						}
						temp = p;
						//down and right
						if (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
						{
							temp = temp.Translate(1, 1);
							positions.Add(temp);

						}
					}

				}
				if (GetPieceAtPosition(p).PieceType == ChessPieceType.Knight && (GetPieceAtPosition(p).Player == byPlayer))
				{
					positions.UnionWith(GetKnightLogic(positions, temp));
				}
			}
			return positions;
		}

		public String listToString(List<ChessMove> list)
		{
			StringBuilder builder = new StringBuilder();
			foreach (ChessMove m in list) // Loop through all strings
			{
				builder.Append(m.ToString()).Append("\n"); // Append string to StringBuilder
			}
			string result = builder.ToString(); // Get string from StringBuilder
			return result;
		}

		#endregion

		#region Piece Logic.
		public ISet<BoardPosition> GetBishopLogic(HashSet<BoardPosition> positions, BoardPosition p){
			BoardPosition temp = p;
		//up and right
			while (PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				temp = temp.Translate(-1, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//down and right
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
			{
				temp = temp.Translate(1, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//down and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
			{
				temp = temp.Translate(1, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//up and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				temp = temp.Translate(-1, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			return positions;

		}
		public ISet<BoardPosition> GetQueenLogic(HashSet<BoardPosition> positions, BoardPosition p){
			BoardPosition temp = p;
			//up and right
			while (PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				temp = temp.Translate(-1, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//down and right
			temp = p;
			while (PositionInBounds(temp.Translate(1, 1)) == true)
			{
				temp = temp.Translate(1, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//down and left
			temp = p;
			while (PositionInBounds(temp.Translate(1, -1)) == true)
			{
				temp = temp.Translate(1, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			//up and left
			temp = p;
			while (PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				temp = temp.Translate(-1, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//up
			while (PositionInBounds(temp.Translate(1, 0)) == true)
			{
				temp = temp.Translate(1, 0);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//right
			while (PositionInBounds(temp.Translate(0, 1)) == true)
			{
				temp = temp.Translate(0, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//down
			while (PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				temp = temp.Translate(-1, 0);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//left
			while (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				temp = temp.Translate(0, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			return positions;
		}
		public ISet<BoardPosition> GetKingLogic(HashSet<BoardPosition> positions, BoardPosition p){
			BoardPosition temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				temp = temp.Translate(-1, 1);
				positions.Add(temp);
			}
			//down and right
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
			{
				temp = temp.Translate(1, 1);
				positions.Add(temp);
			}
			//down and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
			{
				temp = temp.Translate(1, -1);
				positions.Add(temp);
			}
			//up and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				temp = temp.Translate(-1, -1);
				positions.Add(temp);
			}
			temp = p;
			//up
			if (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				temp = temp.Translate(1, 0);
				positions.Add(temp);
			}
			temp = p;
			//right
			if (ChessBoard.PositionInBounds(temp.Translate(0, 1)) == true)
			{
				temp = temp.Translate(0, 1);
				positions.Add(temp);
			}
			temp = p;
			//down
			if (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				temp = temp.Translate(-1, 0);
				positions.Add(temp);
			}
			temp = p;
			//left
			if (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				temp = temp.Translate(0, -1);
				positions.Add(temp);
			}
			return positions;
		}
		public ISet<BoardPosition> GetRookLogic(HashSet<BoardPosition> positions, BoardPosition p){
			BoardPosition temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				temp = temp.Translate(1, 0);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//right
			while (ChessBoard.PositionInBounds(temp.Translate(0, 1)) == true)
			{
				temp = temp.Translate(0, 1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//down
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				temp = temp.Translate(-1, 0);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			temp = p;
			//left
			while (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				temp = temp.Translate(0, -1);
				positions.Add(temp);
				if (!PositionIsEmpty(temp))
				{
					break;
				}
			}
			return positions;
		}
		public ISet<BoardPosition> GetPawnLogic(HashSet<BoardPosition> positions, BoardPosition temp){
			return positions;
		}
		public ISet<BoardPosition> GetKnightLogic(HashSet<BoardPosition> positions, BoardPosition p){
			BoardPosition temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(-2, 1)) == true)
			{
				temp = temp.Translate(-2, 1);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			//down and right
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(2, 1)) == true)
			{
				temp = temp.Translate(2, 1);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			//down and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(2, -1)) == true)
			{
				temp = temp.Translate(2, -1);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			//up and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(-2, -1)) == true)
			{
				temp = temp.Translate(-2, -1);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			temp = p;
			//right and up
			if (ChessBoard.PositionInBounds(temp.Translate(-1, 2)) == true)
			{
				temp = temp.Translate(-1, 2);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			temp = p;
			//right and down
			if (ChessBoard.PositionInBounds(temp.Translate(1, 2)) == true)
			{
				temp = temp.Translate(1, 2);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			temp = p;
			//left and up
			if (ChessBoard.PositionInBounds(temp.Translate(-1, -2)) == true)
			{
				temp = temp.Translate(-1, -2);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			temp = p;
			//left and down
			if (ChessBoard.PositionInBounds(temp.Translate(1, -2)) == true)
			{
				temp = temp.Translate(1, -2);
				positions.Add(temp);
				//if (!PositionIsEmpty(temp))
				//{
				//	break;
				//}
			}
			return positions;
		}

		//used to test if a piece has moved
		public bool containsStartPosition(List<ChessMove> l, BoardPosition startPos)
		{
			foreach (ChessMove m in l)
			{
				if (m.StartPosition.Equals(startPos))
				{
					return true;
				}
			}
			return false;
		}
		public List<ChessMove> possibleCastlingMoves()
		{
			List<ChessMove> castlingMoves = new List<ChessMove>();
			if (currentPlayer == 1)
			{
				//white king hasnt moved
				if (!containsStartPosition(mMoveHistory, new BoardPosition(7, 4)) && GetPieceAtPosition(new BoardPosition(7, 4)).PieceType == ChessPieceType.King)
				{
					//right white rook hasnt moved
					if (!containsStartPosition(mMoveHistory, new BoardPosition(7, 7)) && GetPieceAtPosition(new BoardPosition(7, 7)).PieceType == ChessPieceType.Rook)
					{
						//check if the king is not attacked and the spaces in between are empty
						if (!PositionIsAttacked(new BoardPosition(7, 4), 2) && !PositionIsAttacked(new BoardPosition(7, 5), 2) && !PositionIsAttacked(new BoardPosition(7, 6), 2) && PositionIsEmpty(new BoardPosition(7, 5)) && PositionIsEmpty(new BoardPosition(7, 6)))
						{
							//king
							castlingMoves.Add(new ChessMove(new BoardPosition(7, 4), new BoardPosition(7, 6), ChessMoveType.CastleKingSide));
							//rook
							//castlingMoves.Add(new ChessMove(new BoardPosition(7, 7), new BoardPosition(7, 5), ChessMoveType.CastleKingSide));
						}

					}

					//left rook hasnt moved
					if (!containsStartPosition(mMoveHistory, new BoardPosition(7, 0)) && GetPieceAtPosition(new BoardPosition(7, 0)).PieceType == ChessPieceType.Rook)
					{

						if (!PositionIsAttacked(new BoardPosition(7, 4), 2) && !PositionIsAttacked(new BoardPosition(7, 3), 2) && !PositionIsAttacked(new BoardPosition(7, 2), 2) && PositionIsEmpty(new BoardPosition(7, 3)) && PositionIsEmpty(new BoardPosition(7, 2)) && PositionIsEmpty(new BoardPosition(7, 1)))
						{
							//king
							castlingMoves.Add(new ChessMove(new BoardPosition(7, 4), new BoardPosition(7, 2), ChessMoveType.CastleQueenSide));
							//rook
							//castlingMoves.Add(new ChessMove(new BoardPosition(7, 0), new BoardPosition(7, 3), ChessMoveType.CastleQueenSide));
						}
					}
				}
			}

			if (currentPlayer == 2)
			{
				//black king hasnt moved
				if (!containsStartPosition(mMoveHistory, new BoardPosition(0, 4)) && GetPieceAtPosition(new BoardPosition(0, 4)).PieceType == ChessPieceType.King)
				{
					//right black rook hasnt moved
					if (!containsStartPosition(mMoveHistory, new BoardPosition(0, 7)) && GetPieceAtPosition(new BoardPosition(0, 7)).PieceType == ChessPieceType.Rook)
					{
						//check if the king is not attacked and the spaces in between are empty
						if (!PositionIsAttacked(new BoardPosition(0, 4), 1) && !PositionIsAttacked(new BoardPosition(0, 5), 1) && !PositionIsAttacked(new BoardPosition(0, 6), 1) && PositionIsEmpty(new BoardPosition(0, 5)) && PositionIsEmpty(new BoardPosition(0, 6)))
						{
							//king
							castlingMoves.Add(new ChessMove(new BoardPosition(0, 4), new BoardPosition(0, 6), ChessMoveType.CastleKingSide));
							//rook
							//castlingMoves.Add(new ChessMove(new BoardPosition(0, 7), new BoardPosition(0, 5), ChessMoveType.CastleKingSide));
						}

					}

					//left rook hasnt moved
					if (!containsStartPosition(mMoveHistory, new BoardPosition(0, 0)) && GetPieceAtPosition(new BoardPosition(0, 0)).PieceType == ChessPieceType.Rook)
					{

						if (!PositionIsAttacked(new BoardPosition(0, 4), 1) && !PositionIsAttacked(new BoardPosition(0, 3), 1) && !PositionIsAttacked(new BoardPosition(0, 2), 1) && PositionIsEmpty(new BoardPosition(0, 3)) && PositionIsEmpty(new BoardPosition(0, 2)) && PositionIsEmpty(new BoardPosition(0, 1)))
						{
							//king
							castlingMoves.Add(new ChessMove(new BoardPosition(0, 4), new BoardPosition(0, 2), ChessMoveType.CastleQueenSide));
							//rook
							//castlingMoves.Add(new ChessMove(new BoardPosition(0, 0), new BoardPosition(0, 3), ChessMoveType.CastleQueenSide));
						}
					}
				}
			}
			return castlingMoves;
		}

		#endregion

		#region Private methods.
		/// <summary>
		/// Mutates the board state so that the given piece is at the given position.
		/// </summary>
		private void SetPieceAtPosition(BoardPosition position, ChessPiece piece) {

			byte pieceType = (byte) piece.PieceType;
			int index = position.Row * 4;
			index += position.Col / 2;

			byte square = board[index];
			byte adjPiece = board[index];

			if (position.Col % 2 == 0)
			{
				// then leftmost bits
				adjPiece = (byte) (square & 0b00001111);
				square = (byte) (square & 0b11110000);

				byte player;

				if (piece.Player == 2)
				{
					player = (byte) (square | 0b10000000);
					player = (byte) (player & 0b10000000);
				}

				else
				{
					player = (byte) (square & 0b00000000);
				}

				byte type = (byte )((byte) piece.PieceType & 0b11111111);

				byte newPiece = (byte) (0b11111111 & (type << 4));
				newPiece = (byte) (player | newPiece);

				board[index] = (byte) (newPiece & 0b11111111);

				board[index] = (byte) (adjPiece | newPiece);

			}
			else
			{ // else rightmost bits
				adjPiece = (byte) (square & 0b11110000);
				square = (byte) (square & 0b00001111);

				byte player;

				if (piece.Player == 2)
				{
					player = (byte) (square | 0b00001000);
					player = (byte)(player & 0b00001000);
				}

				else
				{
					player = (byte) (square & 0b00000000);
					//player = (byte)(player & 0b00000000);

				}

				//byte player = (byte)(square & 0b00001000);
				byte type = (byte) ((byte) piece.PieceType & 0b11111111);
				byte newPiece = (byte) (0b11111111 & (type));
				newPiece = (byte)(player | newPiece);

				board[index] = (byte) (newPiece & 0b11111111);
				board[index] = (byte) (adjPiece | newPiece);


			}

		}

		#endregion

		#region Explicit IGameBoard implementations.
		IEnumerable<IGameMove> IGameBoard.GetPossibleMoves() {
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m) {
			ApplyMove(m as ChessMove);
		}
		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;
		#endregion

		#region Hypothetical Functions

		public void hypotheticalApplyMove(ChessMove m)
		{

			//enPassantMovesBlack.Clear();
			//enPassantMovesWhite.Clear();
			ChessPiece tester = GetPieceAtPosition(m.StartPosition);
			//if (1 == 1)
			//{
			//	throw new InvalidOperationException($"There are no captured pieces: {tester.PieceType}, {m.EndPosition}, {m.MoveType}");
			//}
			if (((m.StartPosition.Row == 1 && m.EndPosition.Row == 3) || (m.StartPosition.Row == 6 && m.EndPosition.Row == 4)) && (GetPieceAtPosition(m.StartPosition).PieceType == ChessPieceType.Pawn))
			{
				capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				// Set the moved piece's position to the square it's being moved to
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Pawn, m.Player));
				// Set the moved piece's start position to empty 
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));

			} // End of if double jump

			else
			{

				//enPassantMovesBlack.Clear();
				//enPassantMovesWhite.Clear();
				// If the move is en passant:
				if (m.MoveType == ChessMoveType.EnPassant)
				{

					//enPassantMovesBlack.Clear();
					//enPassantMovesWhite.Clear();
					// If white pawn is capturing a black pawn with en passant
					if (currentPlayer == 1)
					{
						// 1. Set the current player's pawn to the new position
						SetPieceAtPosition(m.EndPosition, new ChessPiece(GetPieceAtPosition(m.StartPosition).PieceType, 1));
						// 2. Add the captured black pawn to captured pieces list
						//capturedPieces.Push(GetPieceAtPosition(m.EndPosition.Translate(1, 0)));
						capturedPieces.Push(new ChessPiece(GetPieceAtPosition(m.EndPosition.Translate(1, 0)).PieceType, 2));
						SetPieceAtPosition(m.EndPosition.Translate(1, 0), new ChessPiece(ChessPieceType.Empty, 0));
						// 3. Set the current player's previous pawn position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
						//player2total--;
					}
					// Else if black pawn is capturing a white pawn with en passant
					else
					{
						// 1. Set the current player's pawn to the new position
						SetPieceAtPosition(m.EndPosition, new ChessPiece(GetPieceAtPosition(m.StartPosition).PieceType, 2));
						// 2. Add the captured black pawn to captured pieces list
						//capturedPieces.Push(GetPieceAtPosition(m.EndPosition.Translate(-1, 0)));
						capturedPieces.Push(new ChessPiece(GetPieceAtPosition(m.EndPosition.Translate(-1, 0)).PieceType, 1));
						SetPieceAtPosition(m.EndPosition.Translate(-1, 0), new ChessPiece(ChessPieceType.Empty, 0));
						// 3. Set the current player's previous pawn position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
						//player1total--;
					}
				} // End of if EnPassant


				if (m.MoveType == ChessMoveType.PawnPromote)
				{
					//capturedPieces.Push(new ChessPiece(0, 0));
					// Set end position of the move to the type of piece chosen to promote to
					//SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PromoType, GetPieceAtPosition(m.StartPosition).Player));
					// Set previous pawn position to empty
					//SetPieceAtPosition(m.StartPosition, new ChessPiece(0, 0));

					// If a piece is not being captured during the pawn promote
					if (GetPieceAtPosition(m.EndPosition).PieceType == ChessPieceType.Empty)
					{
						capturedPieces.Push(new ChessPiece(0, 0));
						// Set end position of the move to the type of piece chosen to promote to
						SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PromoType, GetPieceAtPosition(m.StartPosition).Player));
						// Set previous pawn position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(0, 0));

					}

					// If a piece is being captured during the pawn promote
					else
					{
						// Push the captured piece to the captured pieces stack
						capturedPieces.Push(GetPieceAtPosition(m.EndPosition));

						// Set the moved piece to the end position and make it the type it is being promoted to
						SetPieceAtPosition(m.EndPosition, new ChessPiece(m.PromoType, GetPieceAtPosition(m.StartPosition).Player));
						// Set the pawn's previous position to empty
						SetPieceAtPosition(m.StartPosition, new ChessPiece(0, 0));


					}


				}


				if (m.MoveType == ChessMoveType.CastleKingSide)
				{
					capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));

					//set the rook
					if (currentPlayer == 1)
					{
						SetPieceAtPosition(new BoardPosition(7, 5), GetPieceAtPosition(new BoardPosition(7, 7)));
						SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Empty, 0));
					}
					if (currentPlayer == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 5), GetPieceAtPosition(new BoardPosition(0, 7)));
						SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Empty, 0));
					}
					//set the king
					SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}
				//If the move type is castling queen side
				if (m.MoveType == ChessMoveType.CastleQueenSide)
				{

					capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));

					//set the rook
					if (currentPlayer == 1)
					{
						SetPieceAtPosition(new BoardPosition(7, 3), GetPieceAtPosition(new BoardPosition(7, 0)));
						SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Empty, 0));
					}
					if (currentPlayer == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 3), GetPieceAtPosition(new BoardPosition(0, 0)));
						SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Empty, 0));
					}
					//set the king
					SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}

				else
				{
					//enPassantMovesBlack.Clear();
					//enPassantMovesWhite.Clear();
					if (!PositionIsEmpty(m.StartPosition))
					{
						// If the position contains an enemy piece
						if (!PositionIsEmpty(m.EndPosition))
						{
							// Add newly-captured enemy piece to capturedPieces stack
							capturedPieces.Push(GetPieceAtPosition(m.EndPosition));

						}
						// If the position is empty
						else
						{
							// Add an empty piece to the capturedPieces stack
							capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));
						}

						SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));

						// Set the moved piece's position to the square it's being moved to
						SetPieceAtPosition(m.EndPosition, GetPieceAtPosition(m.StartPosition));
						// Set the moved piece's start position to empty 
						SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));

					}

					//capturedPieces.Push(new ChessPiece(ChessPieceType.Empty, 0));

				}
			}

			m.Player = CurrentPlayer;
			mMoveHistory.Add(m);

		}

		public void hypotheticalUndoLastMove()
		{

			ChessMove lastMove = mMoveHistory[mMoveHistory.Count - 1];

			BoardPosition lastMoveStart = lastMove.StartPosition;//lastMove.StartPosition;
			BoardPosition lastMoveEnd = lastMove.EndPosition;//lastMove.EndPosition;
			int player = lastMove.Player;

			ChessPiece pieceMoved = GetPieceAtPosition(lastMoveEnd);

			ChessPiece possibleCapture = capturedPieces.Pop();

			// If the move being undone did not have a capture
			if (possibleCapture.PieceType == ChessPieceType.Empty)
			{
				// If the last move was a pawn promotion, set piece back to pawn
				if (lastMove.MoveType == ChessMoveType.PawnPromote)
				{

					SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, CurrentPlayer));
					// Set pawn's previous position to empty
					SetPieceAtPosition(lastMoveEnd, new ChessPiece(0, 0));
				}

				else if (lastMove.MoveType == ChessMoveType.CastleKingSide || lastMove.MoveType == ChessMoveType.CastleQueenSide)
				{
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 1)
					{
						//kingside white
						if (lastMoveEnd.Equals(new BoardPosition(7, 6)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 1));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1));
							//SetPieceAtPosition(new BoardPosition(7, 7), GetPieceAtPosition(new BoardPosition(7, 5)));
							SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 2)
					{
						//kingside black
						if (lastMoveEnd.Equals(new BoardPosition(0, 6)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 2));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2));
							//SetPieceAtPosition(new BoardPosition(0, 7), GetPieceAtPosition(new BoardPosition(0, 5)));
							SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 1)
					{
						//Queenside white
						if (lastMoveEnd.Equals(new BoardPosition(7, 2)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 1));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1));
							//SetPieceAtPosition(new BoardPosition(7, 0), GetPieceAtPosition(new BoardPosition(7, 3)));
							SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
					if (GetPieceAtPosition(lastMove.EndPosition).Player == 2)
					{
						//queenside black
						if (lastMoveEnd.Equals(new BoardPosition(0, 2)))
						{
							SetPieceAtPosition(lastMoveStart, new ChessPiece(GetPieceAtPosition(lastMoveEnd).PieceType, 2));
							SetPieceAtPosition(lastMoveEnd, new ChessPiece(ChessPieceType.Empty, 0));
							//reset rook
							SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2));
							//SetPieceAtPosition(new BoardPosition(0, 0), GetPieceAtPosition(new BoardPosition(0, 3)));
							SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
				}


				else
				{
					SetPieceAtPosition(lastMove.StartPosition, new ChessPiece(pieceMoved.PieceType, lastMove.Player));
					SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}


			}

			// If a piece was captured during the last move
			else
			{

				// If the last move was a pawn promotion, set piece back to pawn
				if (lastMove.MoveType == ChessMoveType.PawnPromote)
				{

					SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, CurrentPlayer));
					SetPieceAtPosition(lastMoveEnd, possibleCapture);

				}

				else if (lastMove.MoveType == ChessMoveType.EnPassant)
				{

					if (currentPlayer == 1)
					{
						SetPieceAtPosition(lastMove.EndPosition.Translate(1, 0), possibleCapture);
						SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, 1));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					}

					else
					{
						SetPieceAtPosition(lastMove.EndPosition.Translate(-1, 0), possibleCapture);
						SetPieceAtPosition(lastMoveStart, new ChessPiece(ChessPieceType.Pawn, 2));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					}

				}

				else
				{
					SetPieceAtPosition(lastMoveStart, pieceMoved);
					SetPieceAtPosition(lastMoveEnd, possibleCapture);
				}



			}


			mMoveHistory.RemoveAt(mMoveHistory.Count - 1);



			//if (currentPlayer == 1)
			//{
			//	currentPlayer = 2;
			//}

			//else
			//{
			//	currentPlayer = 1;
			//}
		}
		#endregion

		// You may or may not need to add code to this constructor.
		public ChessBoard() {

		}

		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this() {
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1) {
				throw new ArgumentException("A chess board must have a single king for each player");
			}

			foreach (var position in BoardPosition.GetRectangularPositions(8, 8)) {
				SetPieceAtPosition(position, ChessPiece.Empty);
			}

			int[] values = { 0, 0 };
			foreach (var pos in startingPositions) {
				SetPieceAtPosition(pos.Item1, pos.Item2);
				// TODO: you must calculate the overall advantage for this board, in terms of the pieces
				// that the board has started with. "pos.Item2" will give you the chess piece being placed
				// on this particular position.
				if(pos.Item2.Player == 1)
				{
					if (pos.Item2.PieceType == ChessPieceType.Pawn)
					{
						player1total++;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Knight || pos.Item2.PieceType == ChessPieceType.Bishop)
					{
						player1total += 3;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Rook)
					{
						player1total += 5;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Queen)
					{
						player1total += 9;
					}
				}
				else
				{
					if (pos.Item2.PieceType == ChessPieceType.Pawn)
					{
						player2total++;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Knight || pos.Item2.PieceType == ChessPieceType.Bishop)
					{
						player2total += 3;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Rook)
					{
						player2total += 5;
					}

					else if (pos.Item2.PieceType == ChessPieceType.Queen)
					{
						player2total += 9;
					}
				}
			}

			p1Stack.Pop();
			p2Stack.Pop();
			p1Stack.Push(player1total);
			p2Stack.Push(player2total);
		}
	}
}
