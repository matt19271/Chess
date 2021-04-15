using System;
using System.Collections.Generic;
using Cecs475.BoardGames.Model;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents a single move to be applied to a chess board.
	/// </summary>
	public class ChessMove : IGameMove, IEquatable<ChessMove> {
		// You can add additional fields, properties, and methods as you find
		// them necessary, but you cannot MODIFY any of the existing implementations.

		/// <summary>
		/// The starting position of the move.
		/// </summary>
		public BoardPosition StartPosition { get; }

		/// <summary>
		/// The ending position of the move.
		/// </summary>
		public BoardPosition EndPosition { get; }

		/// <summary>
		/// The type of move being applied.
		/// </summary>
		public ChessMoveType MoveType { get; }

		// You must set this property when applying a move.
		public int Player { get; set; }

		public ChessPieceType PromoType { get; set; }

		/// <summary>
		/// Constructs a ChessMove that moves a piece from one position to another
		/// </summary>
		/// <param name="start">the starting position of the piece to move</param>
		/// <param name="end">the position where the piece will end up</param>
		/// <param name="moveType">the type of move represented</param>
		public ChessMove(BoardPosition start, BoardPosition end, ChessMoveType moveType = ChessMoveType.Normal) {
			StartPosition = start;
			EndPosition = end;
			MoveType = moveType;
			PromoType = ChessPieceType.Empty;
		}

		public ChessMove(BoardPosition start, BoardPosition end, ChessPieceType promoType, ChessMoveType moveType = ChessMoveType.PawnPromote)
		{
			StartPosition = start;
			EndPosition = end;
			MoveType = moveType;
			PromoType = promoType;
		}

		// TODO: You must write this method.
		public virtual bool Equals(ChessMove other) {
			// Most chess moves are equal to each other if they have the same start and end position.
			// PawnPromote moves must also be promoting to the same piece type.
			if(this.StartPosition == other.StartPosition && this.EndPosition == other.EndPosition)
			{
				if (this.PromoType != other.PromoType)//this.MoveType == ChessMoveType.PawnPromote && other.MoveType == ChessMoveType.PawnPromote)
				{
					//if (this.PromoType == other.PromoType)
					//{
					//	return true;
					//}
					//else
					//{
						return false;
					//}
				}
				return true;		
			}
			else
			{
				return false;
			}
		}


		public static ChessPieceType StringToPromoType(string promo) {

			promo = promo.ToLower();
			promo = promo.Trim();
			switch (promo)
			{
				case "queen":
					return ChessPieceType.Queen;
				case "bishop":
					return ChessPieceType.Bishop;
				case "knight":
					return ChessPieceType.Knight;
				case "rook":
					return ChessPieceType.Rook;
				default:
					return ChessPieceType.Pawn;
			}
		
		}


		// Equality methods.
		bool IEquatable<IGameMove>.Equals(IGameMove other) {
			ChessMove m = other as ChessMove;
			return this.Equals(m);
		}

		public override bool Equals(object other) {
			return Equals(other as ChessMove);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = StartPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ EndPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)MoveType;
				return hashCode;
			}
		}

		public override string ToString() {
			return $"{StartPosition} to {EndPosition} + {PromoType} + type {MoveType}";
		}


		public IEnumerable<BoardPosition> BishopMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			//Translate (ex 1 up 1 right) until inbounds == false and flip directions from starting positions until youve gone all 4 ways
			BoardPosition temp = p;
			//up and right
			while(ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				temp = temp.Translate(-1,1);
				positions.Add(temp);
			}
			//down and right
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
			{
				positions.Add(temp);
			}
			//up and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				positions.Add(temp);
			}
			return positions;

		}

		public IEnumerable<BoardPosition> QueenMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			//Translate (ex 1 up 1 right) until inbounds == false and flip directions from starting positions until youve gone all 8 ways
			BoardPosition temp = p;
			//up and right
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and right
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
			{
				positions.Add(temp);
			}
			//up and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//up
			while (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//right
			while (ChessBoard.PositionInBounds(temp.Translate(0, 1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//down
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//left
			while (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				positions.Add(temp);
			}
			return positions;
		}

		public IEnumerable<BoardPosition> KingMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			//Translate (ex 1 up 1 right) until inbounds == false and flip directions from starting positions until youve gone all 8 ways
			BoardPosition temp = p;
			//up and right
			if (ChessBoard.PositionInBounds(temp.Translate(-1, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and right
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(1, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(1, -1)) == true)
			{
				positions.Add(temp);
			}
			//up and left
			temp = p;
			if (ChessBoard.PositionInBounds(temp.Translate(-1, -1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//up
			if (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//right
			if (ChessBoard.PositionInBounds(temp.Translate(0, 1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//down
			if (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//left
			if (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				positions.Add(temp);
			}
			return positions;
		}

		public IEnumerable<BoardPosition> RookMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			//Translate (ex 1 up 1 right) until inbounds == false and flip directions from starting positions until youve gone all 4 ways
			BoardPosition temp = p;
			//up
			while (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//right
			while (ChessBoard.PositionInBounds(temp.Translate(0, 1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//down
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//left
			while (ChessBoard.PositionInBounds(temp.Translate(0, -1)) == true)
			{
				positions.Add(temp);
			}
			return positions;
		}

		public IEnumerable<BoardPosition> PawnMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			BoardPosition temp = p;
			/*
			 * player one moves up the board
			 * must write if statement for if its the starting turn then add move forward two
			 * also if an enemy is to the diagonal then a possible move is there
			 * also if enpassant is possible add that as well
			*/
			while (ChessBoard.PositionInBounds(temp.Translate(1, 0)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			/*
			 * player two moves down the board
			 * must write if statement for if its the starting turn then add move forward two
			 * also if an enemy is to the diagonal then a possible move is there
			 * also if enpassant is possible add that as well
			*/
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 0)) == true)
			{
				positions.Add(temp);
			}
			return positions;
		}

		public IEnumerable<BoardPosition> KnightMoves(BoardPosition p)
		{
			List<BoardPosition> positions = new List<BoardPosition>();
			//Translate (ex 1 up 1 right) until inbounds == false and flip directions from starting positions until youve gone all 8 ways
			BoardPosition temp = p;
			//up and right
			while (ChessBoard.PositionInBounds(temp.Translate(-2, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and right
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(2, 1)) == true)
			{
				positions.Add(temp);
			}
			//down and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(2, -1)) == true)
			{
				positions.Add(temp);
			}
			//up and left
			temp = p;
			while (ChessBoard.PositionInBounds(temp.Translate(-2, -1)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//right and up
			while (ChessBoard.PositionInBounds(temp.Translate(-1, 2)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//right and down
			while (ChessBoard.PositionInBounds(temp.Translate(1, 2)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//left and up
			while (ChessBoard.PositionInBounds(temp.Translate(-1, -2)) == true)
			{
				positions.Add(temp);
			}
			temp = p;
			//left and down
			while (ChessBoard.PositionInBounds(temp.Translate(1, -2)) == true)
			{
				positions.Add(temp);
			}
			return positions;
		}
	}
}
