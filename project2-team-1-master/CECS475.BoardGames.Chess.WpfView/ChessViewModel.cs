using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Cecs475.BoardGames.WpfView;
using System;

using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;

namespace CECS475.BoardGames.Chess.WpfView
{

	public class ChessSquare : INotifyPropertyChanged
	{
		private int mPlayer;
		private ChessPiece mPiece;
		/// <summary>
		/// The player that has a piece in the given square, or 0 if empty.
		/// </summary>
		public ChessPiece Player
		{
			get { return mPiece; }
			set
			{
				if (!value.Equals(mPiece))
				{
					mPiece = value;
					OnPropertyChanged(nameof(Player));
				}
			}
		}

		public bool KingInCheck
		{
			get { return mIsCheck; }
			set
			{
				if (value != mIsCheck)
				{
					mIsCheck = value;
					OnPropertyChanged(nameof(KingInCheck));
				}
			}
		}

		/// <summary>
		/// The position of the square.
		/// </summary>
		public BoardPosition Position
		{
			get; set;
		}


		private bool mIsHighlighted;
		private bool mIsGreenHighlighted;
		private bool mIsSelected;
		private bool mIsCheck;
		/// <summary>
		/// Whether the square should be highlighted because of a user action.
		/// </summary>
		public bool IsHighlighted
		{
			get { return mIsHighlighted; }
			set
			{
				if (value != mIsHighlighted)
				{
					mIsHighlighted = value;
					OnPropertyChanged(nameof(IsHighlighted));
				}
			}
		}

		public bool IsGreenHighlighted
		{
			get { return mIsGreenHighlighted; }
			set
			{
				if (value != mIsGreenHighlighted)
				{
					mIsGreenHighlighted = value;
					OnPropertyChanged(nameof(IsGreenHighlighted));
				}
			}
		}

		public bool IsSelected
		{
			get { return mIsSelected; }
			set
			{
				if (value != mIsSelected)
				{
					mIsSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
	public class ChessViewModel : INotifyPropertyChanged, IGameViewModel
	{

		public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

		public bool CanUndo => mBoard.MoveHistory.Any();

		public event EventHandler GameFinished;
		public event PropertyChangedEventHandler PropertyChanged;

		public bool SelectedState
		{
			get; set;
		}

		public String Promote
		{
			get { return mPromote; }
			set
			{
				if (value != mPromote)
				{
					mPromote = value;
					OnPropertyChanged(nameof(Promote));
				}
			}
		}

		public String mPromote;
		private ChessBoard mBoard;
		private ObservableCollection<ChessSquare> mSquares;
		public ChessViewModel()
		{
			mBoard = new ChessBoard();

			SelectedSquare = null;

			Promote = null;


			// Initialize the squares objects based on the board's initial state.
			mSquares = new ObservableCollection<ChessSquare>(
				BoardPosition.GetRectangularPositions(8, 8)
				.Select(pos => new ChessSquare()
				{
					Position = pos,
					Player = mBoard.GetPieceAtPosition(pos)
				})
			);

			PossibleMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.EndPosition //most likely end position as thats where we are trying to go?
			);

			PossibleStartMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.StartPosition
			);

		}



		/// <summary>
		/// Applies a move for the current player at the given position.
		/// </summary>
		public void ApplyMove(BoardPosition position)
		{
			var possMoves = mBoard.GetPossibleMoves() as IEnumerable<ChessMove>;
			// Validate the move as possible.
			foreach (var move in possMoves)
			{
				if (SelectedSquare.Position.Equals(move.StartPosition) && move.EndPosition.Equals(position))
				{
					if (move.MoveType == ChessMoveType.PawnPromote)
					{
						var window = new PawnPromotionWindow(this, move.StartPosition, move.EndPosition);
						window.ShowDialog();
						Promote = window.promotePicked;
						ChessMove m = new ChessMove(move.StartPosition, move.EndPosition, ChessMove.StringToPromoType(Promote), ChessMoveType.PawnPromote);
						mBoard.ApplyMove(m);
						foreach (var s in mSquares)
						{
							s.KingInCheck = false;
						}
						SelectedState = false;
						break;
					}

					else
					{
						mBoard.ApplyMove(move);
						foreach (var s in mSquares)
						{
							s.KingInCheck = false;
						}
						SelectedState = false;
						break;
					}

				}
			}

			RebindState();

			if (mBoard.IsFinished)
			{
				GameFinished?.Invoke(this, new EventArgs());
			}
		}

		public HashSet<BoardPosition> PossibleStartMoves
		{
			get; private set;
		}


		public HashSet<BoardPosition> PossibleMoves
		{
			get; private set;
		}

		public int CurrentPlayer
		{
			get { return mBoard.CurrentPlayer; }
		}

		private void RebindState()
		{
			// Rebind the possible moves, now that the board has changed.
			PossibleMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.EndPosition//once again endposition
			);

			PossibleStartMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.StartPosition
			);

			// Update the collection of squares by examining the new board state.
			var newSquares = BoardPosition.GetRectangularPositions(8, 8);
			int i = 0;
			foreach (var pos in newSquares)
			{
				mSquares[i].Player = mBoard.GetPieceAtPosition(pos);
				i++;
			}

			foreach (var s in mSquares)
			{
				if (IsCheck && s.Player.PieceType == ChessPieceType.King && s.Player.Player == CurrentPlayer)
				{
					s.KingInCheck = true;
				}
				else
				{
					s.KingInCheck = false;
				}
			}

			OnPropertyChanged(nameof(BoardAdvantage));
			OnPropertyChanged(nameof(CurrentPlayer));
			OnPropertyChanged(nameof(CanUndo));
		}

		private void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public ObservableCollection<ChessSquare> Squares
		{
			get { return mSquares; }
		}

		public ChessSquare SelectedSquare
		{
			get; set;
		}

		public bool IsCheck
		{
			get
			{
				return mBoard.IsCheck;
			}

			set
			{
				IsCheck = mBoard.IsCheck;
			}

		}


		public HashSet<BoardPosition> GetPossMovesFromPos(BoardPosition pos)
		{
			var possMoves = mBoard.GetPossibleMoves() as IEnumerable<ChessMove>;
			// Validate the move as possible.
			HashSet<BoardPosition> p = new HashSet<BoardPosition>();
			foreach (var move in possMoves)
			{
				if (move.StartPosition.Equals(pos))
				{
					p.Add(move.EndPosition);
				}
			}

			return p;
		}

		public BoardPosition GetSelectedSquarePos()
		{
			return SelectedSquare.Position;
		}

		public ChessSquare GetSquareAtPos(BoardPosition pos)
		{
			foreach (var s in mSquares)
			{
				if (s.Position.Equals(pos))
				{
					return s;
				}
			}

			return null;
		}

		public void UndoMove()
		{
			if (CanUndo)
			{
				mBoard.UndoLastMove();
				RebindState();
			}
		}
	}
}

