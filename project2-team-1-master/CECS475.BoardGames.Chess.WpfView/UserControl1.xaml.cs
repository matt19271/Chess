using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cecs475.BoardGames.WpfView;

namespace CECS475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IWpfGameView
    {

        public UserControl1()
        {
            InitializeComponent();
            
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;

            // If a square is not already selected, that means we are choosing a piece to move
            if (vm.SelectedSquare == null)
            {
                if (vm.PossibleStartMoves.Contains(square.Position))
                {
                    square.IsGreenHighlighted = true;
                }
            }
            // Else if a square is already selected, that means we are choosing a spot to move a piece to
            else
            {
                if (vm.GetPossMovesFromPos(vm.SelectedSquare.Position).Contains(square.Position))
                {
                    square.IsGreenHighlighted = true;
                }
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsHighlighted = false;
            square.IsGreenHighlighted = false;
        }
        
        public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;
        public Control ViewControl => this;

        public IGameViewModel ViewModel => ChessViewModel;

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;

            // If a square is not already selected by the player
            if (vm.SelectedSquare == null)
            {
                // If the square clicked has a piece that belongs to the current player
                 if (square.Player.Player == vm.CurrentPlayer && vm.PossibleStartMoves.Contains(square.Position))
                 {
                    square.IsSelected = true;
                    square.IsHighlighted = true;
                    vm.SelectedSquare = square;
                    vm.SelectedState = true;
                 }
            }
           
            // Else if a square is already selected
            else
            {
                // If the user clicks on the square they just selected, it will deselect the square
                if (vm.SelectedSquare == square)
                {
                    square.IsSelected = false;
                    square.IsHighlighted = false;
                    vm.SelectedSquare = null;
                    vm.SelectedState = false;
                }
                // Else if a starting square is selected by the player, the player can select another square if it is a
                // possible end move of the starting square
                else
                {
                    if (vm.GetPossMovesFromPos(vm.SelectedSquare.Position).Contains(square.Position))
                    {
                        square.IsGreenHighlighted = false;
                        vm.ApplyMove(square.Position);
                        square.IsHighlighted = false;
                        vm.GetSquareAtPos(vm.SelectedSquare.Position).IsSelected = false;
                        vm.SelectedSquare = null;
                        
                    }
                    else
                    {
                        foreach (var sq in vm.Squares)
                        {
                            sq.IsSelected = false;
                            vm.SelectedSquare = null;
                        }
                    }
                }
            }

           
        }
    }
}
