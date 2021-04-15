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
using System.Windows.Shapes;

using Cecs475.BoardGames.Model;


namespace CECS475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for PawnPromotionWindow.xaml
    /// </summary>
    public partial class PawnPromotionWindow : Window
    {
        public PawnPromotionWindow(ChessViewModel vm, BoardPosition start, BoardPosition end)
        {
            InitializeComponent();
            wCurrentPlayer = vm.CurrentPlayer;
            this.DataContext = this;
            this.WindowStyle = WindowStyle.None;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            promotePicked = null;
        }

        private int wCurrentPlayer;

        public String promotePicked
        {
            get; set;
        }

        public int CurrentPlayer
        {
            get
            {
                return wCurrentPlayer;
            }
        }

        private void Button_Click_Knight(object sender, RoutedEventArgs e)
        {
            var vm = FindResource("vm") as ChessViewModel;
            promotePicked = "Knight";
            this.Hide();
        }

        private void Button_Click_Bishop(object sender, RoutedEventArgs e)
        {
            var vm = FindResource("vm") as ChessViewModel;
            promotePicked = "Bishop";
            this.Hide();
        }

        private void Button_Click_Rook(object sender, RoutedEventArgs e)
        {
            var vm = FindResource("vm") as ChessViewModel;
            promotePicked = "Rook";
            this.Hide();
        }
        private void Button_Click_Queen(object sender, RoutedEventArgs e)
        {
            var vm = FindResource("vm") as ChessViewModel;
            //vm.Promote = "Queen";
            promotePicked = "Queen";
            this.Hide();
        }





    }

    
}
